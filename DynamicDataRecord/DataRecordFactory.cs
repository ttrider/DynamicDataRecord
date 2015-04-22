using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace TTRider.DynamicDataRecord
{
    public static class DataRecordFactory
    {
        const string RecordInitializeMethod = "__Initialize";
        static readonly ConcurrentDictionary<string, Binder> Binders = new ConcurrentDictionary<string, Binder>();
        static readonly AssemblyBuilder AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
            new AssemblyName("DataRecordFactory"), AssemblyBuilderAccess.RunAndSave);
        static readonly Regex NameCleanup = new Regex(@"[^\w^_]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static readonly ModuleBuilder ModuleBuilder = AssemblyBuilder.DefineDynamicModule("DataRecordFactory");


        static string GetKey(IDataRecord dataRecord)
        {
            var sb = new StringBuilder("Binder:");
            sb.Append(dataRecord.FieldCount);
            for (var i = 0; i < dataRecord.FieldCount; i++)
            {
                sb.Append("-");
                sb.Append(dataRecord.GetName(i));
                sb.Append(":");
                sb.Append(dataRecord.GetDataTypeName(i));
            }
            return sb.ToString();
        }


        public static Binder GetRecordBinder(string className, IDataRecord dataRecord)
        {
            if (string.IsNullOrWhiteSpace(className)) throw new ArgumentNullException("className");
            if (dataRecord == null) throw new ArgumentNullException("dataRecord");
            return Binders.GetOrAdd(GetKey(dataRecord), k => new Binder(className, dataRecord));
        }

        public static dynamic BindRecord(string className, IDataRecord dataRecord)
        {
            if (dataRecord == null) throw new ArgumentNullException("dataRecord");
            var binder = GetRecordBinder(className, dataRecord);
            return binder.Bind(dataRecord);
        }


        public class Binder
        {
            public string Name { get; private set; }
            public Type Type { get; private set; }

            private readonly MethodInfo initialize;

            internal Binder(string className, IDataRecord record)
            {
                if (string.IsNullOrWhiteSpace(className)) throw new ArgumentNullException("className");
                if (record == null) throw new ArgumentNullException("record");

                this.Name = className;
                this.Type = BuildType(className, record);
                this.initialize = this.Type.GetMethod(RecordInitializeMethod);
            }

            Type BuildType(string name, IDataRecord record)
            {
                var typeBuilder = ModuleBuilder.DefineType(name, TypeAttributes.Class | TypeAttributes.Public);

                var properties = EnumerateProperties(record).ToArray();

                var init = typeBuilder.DefineMethod(RecordInitializeMethod, MethodAttributes.Public, null,
                    properties.Select(p=>p.Item2).ToArray());

                var ctorIl = init.GetILGenerator();

                var ind = 1;
                foreach (var prop in properties)
                {
                    var fld = typeBuilder.DefineField(prop.Item1, prop.Item2, FieldAttributes.Public);
                    ctorIl.Emit(OpCodes.Ldarg_0);
                    ctorIl.Emit(OpCodes.Ldarg, ind++);
                    ctorIl.Emit(OpCodes.Stfld, fld);
                }
                ctorIl.Emit(OpCodes.Ret);
                return typeBuilder.CreateType();
            }

            IEnumerable<Tuple<string, Type>> EnumerateProperties(IDataRecord record)
            {
                for (var i = 0; i < record.FieldCount; i++)
                {
                    yield return new Tuple<string, Type>(CleanupName(record.GetName(i)),record.GetFieldType(i));
                }
            }

            string CleanupName(string name)
            {
                if (string.IsNullOrWhiteSpace(name)) return "_"+Guid.NewGuid().ToString("N");
                name = NameCleanup.Replace(name, "_");
                if (char.IsDigit(name, 0))
                {
                    return "_" + name;
                }
                return name;
            }

            public dynamic Bind(IDataRecord dataRecord)
            {
                if (dataRecord == null) throw new ArgumentNullException("dataRecord");

                var item = Activator.CreateInstance(this.Type);
                var values = new object[dataRecord.FieldCount];
                dataRecord.GetValues(values);
                this.initialize.Invoke(item, values);
                return item;
            }

            public dynamic CreateCollection()
            {
                var listType = typeof (List<>).MakeGenericType(this.Type);
                var list = Activator.CreateInstance(listType);
                return list;
            }
            public RecordsContainer CreateContainer()
            {
                var listType = typeof(List<>).MakeGenericType(this.Type);
                var list = Activator.CreateInstance(listType);

                return new RecordsContainer(list);
            }
        }

        public class RecordsContainer
        {
            public static RecordsContainer Empty()
            {
              return new RecordsContainer(new List<object>());  
            } 

            internal RecordsContainer(dynamic recordsList)
            {
                this.Records = recordsList;
            }

            public void Add(dynamic record)
            {
                this.Records.Add(record);
            }

            [XmlArray("Records")]
            [XmlArrayItem("Record")]
            public dynamic Records { get; private set; }
        }
    }
}
