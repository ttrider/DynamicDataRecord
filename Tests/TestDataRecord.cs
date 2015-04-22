using System;
using System.Collections.Generic;
using System.Data;

namespace Tests
{
    public class TestDataRecord : IDataRecord
    {
        readonly List<Tuple<string, object>> values = new List<Tuple<string, object>>();

        public TestDataRecord Add<T>(string name, T value)
        {
            values.Add(new Tuple<string, object>(name, value));
            return this;
        }


        public string GetName(int i)
        {
            return values[i].Item1;
        }

        public string GetDataTypeName(int i)
        {
            return GetFieldType(i).Name;
        }

        public Type GetFieldType(int i)
        {
            var val = values[i].Item2;
            if (val == null) return typeof (object);
            return values[i].Item2.GetType();
        }

        public object GetValue(int i)
        {
            return values[i].Item2;
        }

        public int GetValues(object[] valuesArray)
        {
            var i = 0;
            for (; i < valuesArray.Length && i< this.values.Count; i++)
            {
                valuesArray[i] = GetValue(i);
            }
            return i;
        }

        public int GetOrdinal(string name)
        {
            return values.FindIndex(i => i.Item1 == name);
        }

        public bool GetBoolean(int i)
        {
            return (bool) GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }

        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        public float GetFloat(int i)
        {
            return (float)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        public IDataReader GetData(int i)
        {
            return null;
        }

        public bool IsDBNull(int i)
        {
            return GetValue(i)==null;
        }

        public int FieldCount { get { return values.Count; } }

        object IDataRecord.this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        object IDataRecord.this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }
    }
}
