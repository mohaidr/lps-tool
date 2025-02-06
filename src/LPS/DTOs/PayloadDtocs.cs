using LPS.Domain.LPSSession;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.DTOs
{
    public class PayloadDto
    {
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PayloadDto()
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }
        public Payload.PayloadType? Type { get; set; }
        public string Raw { get; set; }
        public string File { get; set; }
        public MultipartContentDto Multipart { get; set; }
    }

    public class MultipartContentDto
    {
        public MultipartContentDto()
        {
            Fields = [];
            Files = [];
        }
        public List<TextFieldDto> Fields { get; set; } = new();
        public List<FileFieldDto> Files { get; set; } = new();
    }
    public class TextFieldDto
    {
        // Parameterless constructor for deserialization
        public TextFieldDto()
        {
            Name = string.Empty;
            Value = string.Empty;
            ContentType = string.Empty;
        }

        // Parameterized constructor for convenience
        public TextFieldDto(string name, string value, string contentType)
        {
            Name = name;
            Value = value;
            ContentType = contentType;
        }

        public string Name { get; set; }
        public string Value { get; set; }
        public string ContentType { get; set; }
    }

    public class FileFieldDto
    {
        public FileFieldDto()
        {
            Name = string.Empty;
            Path = string.Empty;
            ContentType = string.Empty;
        }
        public FileFieldDto(string name, string path, string contentType)
        {
            Name = name;
            Path = path;
            ContentType = contentType;
        }
        public string Name { get; set; }
        public string Path { get; set; }
        public string ContentType { get; set; }
    }

}
