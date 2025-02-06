using System;
using System.Collections.Generic;
using System.Net.Http;
using LPS.Domain.Domain.Common.Interfaces;

namespace LPS.Domain.LPSSession
{
    public class Payload: IValueObject
    {
        public enum PayloadType
        {
            Raw,
            Multipart,
            Binary
        }

        public MultiPart Multipart { get; private set; }   
        public PayloadType Type { get; private set; }
        public string RawValue { get; private set; }
        public byte[] BinaryValue { get; private set; }

        private Payload(PayloadType type)
        {
            Type = type;
        }

        public static Payload CreateRaw(string rawValue) =>
            new(PayloadType.Raw) { RawValue = rawValue };

        public static Payload CreateBinary(byte[] binaryValue) =>
            new(PayloadType.Binary) 
            { 
                BinaryValue = binaryValue 
            };

        public static Payload CreateMultipart(List<TextField> fields, List<FileField> files) =>
            new(PayloadType.Multipart)
            {
                Multipart = new (fields, files)
            };
    }

    public class MultiPart(List<TextField> textFields, List<FileField> fileFields)
    {
        public List<TextField> Fields { get; private set; } = textFields ?? [];
        public List<FileField> Files { get; private set; } = fileFields ?? [];
    }

    public class TextField(string name, string value, string contentType)
    {
        public string Name { get; private set; } = name;
        public string Value { get; private set; } = value;
        public string ContentType { get; private set; } = contentType;
    }

    public class FileField(string name, string contentType, object content)
    {
        public string Name { get; private set; } = name;
        public string ContentType { get; private set; } = contentType;
        public object Content { get; private set; } = content;
    }
}
