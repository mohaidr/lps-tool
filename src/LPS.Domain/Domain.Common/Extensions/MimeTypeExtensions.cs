using System;
using System.Collections.Generic;

namespace LPS.Domain.Common
{
    public enum MimeType
    {
        // Existing types
        ImageJpeg,
        ImagePng,
        ApplicationPdf,
        TextPlain,
        ApplicationMsWord,
        ApplicationVndMsExcel,
        ApplicationVndOpenXmlFormatsOfficedocumentSpreadsheetmlSheet,
        ApplicationVndMsPowerpoint,
        ApplicationVndOpenXmlFormatsOfficedocumentPresentationmlPresentation,
        ApplicationXml,
        TextXml,
        RawXml,
        TextJavascript,
        ApplicationJavascript,
        ApplicationXJavascript,
        TextCss,
        TextHtml,
        ApplicationJson,
        TextCsv, // CSV file
        ApplicationOctetStream,
        ApplicationZip,
        ApplicationXGzip,
        ApplicationXRarCompressed,
        AudioMpeg,
        AudioWav,
        AudioOgg,
        VideoMp4,
        VideoAvi,
        VideoXMatroska,
        FontTtf,
        FontWoff,
        FontWoff2,
        ApplicationXMsdownload, // Executables
        ApplicationXSh,
        ApplicationVndOasisOpenDocumentSpreadsheet, // .ods
        ApplicationVndOasisOpenDocumentText,        // .odt
        ApplicationXHdf,                            // HDF
        ApplicationNetCdf,                          // NetCDF
        ApplicationXShapefile,                      // Shapefiles
        ApplicationGeoJson,                          // GeoJSON
        Unknown // Unknown content types
    }

    public static class MimeTypeExtensions
    {
        private static readonly Dictionary<MimeType, string> MimeTypeToExtension = new()
        {
            { MimeType.ImageJpeg, ".jpg" },
            { MimeType.ImagePng, ".png" },
            { MimeType.ApplicationPdf, ".pdf" },
            { MimeType.TextPlain, ".txt" },
            { MimeType.ApplicationMsWord, ".doc" },
            { MimeType.ApplicationVndMsExcel, ".xls" },
            { MimeType.ApplicationVndOpenXmlFormatsOfficedocumentSpreadsheetmlSheet, ".xlsx" },
            { MimeType.ApplicationVndMsPowerpoint, ".ppt" },
            { MimeType.ApplicationVndOpenXmlFormatsOfficedocumentPresentationmlPresentation, ".pptx" },
            { MimeType.ApplicationXml, ".xml" },
            { MimeType.TextXml, ".xml" },
            { MimeType.RawXml, ".xml" },
            { MimeType.TextJavascript, ".js" },
            { MimeType.ApplicationJavascript, ".js" },
            { MimeType.ApplicationXJavascript, ".js" },
            { MimeType.TextCss, ".css" },
            { MimeType.TextHtml, ".html" },
            { MimeType.ApplicationJson, ".json" },
            { MimeType.TextCsv, ".csv" },
            { MimeType.ApplicationOctetStream, ".bin" },
            { MimeType.ApplicationZip, ".zip" },
            { MimeType.ApplicationXGzip, ".gz" },
            { MimeType.ApplicationXRarCompressed, ".rar" },
            { MimeType.AudioMpeg, ".mp3" },
            { MimeType.AudioWav, ".wav" },
            { MimeType.AudioOgg, ".ogg" },
            { MimeType.VideoMp4, ".mp4" },
            { MimeType.VideoAvi, ".avi" },
            { MimeType.VideoXMatroska, ".mkv" },
            { MimeType.FontTtf, ".ttf" },
            { MimeType.FontWoff, ".woff" },
            { MimeType.FontWoff2, ".woff2" },
            { MimeType.ApplicationXMsdownload, ".exe" },
            { MimeType.ApplicationXSh, ".sh" },
            { MimeType.ApplicationVndOasisOpenDocumentSpreadsheet, ".ods" },
            { MimeType.ApplicationVndOasisOpenDocumentText, ".odt" },
            { MimeType.ApplicationXHdf, ".hdf" },
            { MimeType.ApplicationNetCdf, ".nc" },
            { MimeType.ApplicationXShapefile, ".shp" },
            { MimeType.ApplicationGeoJson, ".geojson" }
        };
        // Mapping content type to MIME type
        private static readonly Dictionary<string, MimeType> ContentTypeToMimeType = new(StringComparer.OrdinalIgnoreCase)
        {
            { "image/jpeg", MimeType.ImageJpeg },
            { "image/png", MimeType.ImagePng },
            { "application/pdf", MimeType.ApplicationPdf },
            { "text/plain", MimeType.TextPlain },
            { "application/msword", MimeType.ApplicationMsWord },
            { "application/vnd.ms-excel", MimeType.ApplicationVndMsExcel },
            { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", MimeType.ApplicationVndOpenXmlFormatsOfficedocumentSpreadsheetmlSheet },
            { "application/vnd.ms-powerpoint", MimeType.ApplicationVndMsPowerpoint },
            { "application/vnd.openxmlformats-officedocument.presentationml.presentation", MimeType.ApplicationVndOpenXmlFormatsOfficedocumentPresentationmlPresentation },
            { "application/xml", MimeType.ApplicationXml },
            { "text/xml", MimeType.TextXml },
            { "application/raw+xml", MimeType.RawXml },
            { "text/javascript", MimeType.TextJavascript },
            { "application/javascript", MimeType.ApplicationJavascript },
            { "application/x-javascript", MimeType.ApplicationXJavascript },
            { "text/css", MimeType.TextCss },
            { "text/html", MimeType.TextHtml },
            { "application/json", MimeType.ApplicationJson },
            { "text/csv", MimeType.TextCsv },
            { "application/octet-stream", MimeType.ApplicationOctetStream },
            { "application/zip", MimeType.ApplicationZip },
            { "application/x-gzip", MimeType.ApplicationXGzip },
            { "application/x-rar-compressed", MimeType.ApplicationXRarCompressed },
            { "audio/mpeg", MimeType.AudioMpeg },
            { "audio/wav", MimeType.AudioWav },
            { "audio/ogg", MimeType.AudioOgg },
            { "video/mp4", MimeType.VideoMp4 },
            { "video/x-msvideo", MimeType.VideoAvi },
            { "video/x-matroska", MimeType.VideoXMatroska },
            { "font/ttf", MimeType.FontTtf },
            { "font/woff", MimeType.FontWoff },
            { "font/woff2", MimeType.FontWoff2 },
            { "application/x-msdownload", MimeType.ApplicationXMsdownload },
            { "application/x-sh", MimeType.ApplicationXSh },
            { "application/vnd.oasis.opendocument.spreadsheet", MimeType.ApplicationVndOasisOpenDocumentSpreadsheet },
            { "application/vnd.oasis.opendocument.text", MimeType.ApplicationVndOasisOpenDocumentText },
            { "application/x-hdf", MimeType.ApplicationXHdf },
            { "application/netcdf", MimeType.ApplicationNetCdf },
            { "application/x-shapefile", MimeType.ApplicationXShapefile },
            { "application/geo+json", MimeType.ApplicationGeoJson }
        };

        private static readonly Dictionary<string, MimeType> KeywordToMimeType = new(StringComparer.OrdinalIgnoreCase)
        {
            { "JSON", MimeType.ApplicationJson },
            { "XML", MimeType.ApplicationXml },
            { "Text", MimeType.TextPlain },
            { "JPEG", MimeType.ImageJpeg },
            { "PNG", MimeType.ImagePng },
            { "HTML", MimeType.TextHtml },
            { "PDF", MimeType.ApplicationPdf },
            { "JS", MimeType.ApplicationJavascript },
            { "CSS", MimeType.TextCss },
            { "CSV", MimeType.TextCsv }, // Keyword for CSV
            // Add more mappings as needed
        };
        public static string ToFileExtension(this MimeType mimeType)
        {
            if (MimeTypeToExtension.TryGetValue(mimeType, out string extension))
            {
                return extension;
            }

            return ".bin"; // Default extension
        }
        public static MimeType FromContentType(string contentType)
        {
            if (contentType != null && ContentTypeToMimeType.TryGetValue(contentType, out MimeType mimeType))
            {
                return mimeType;
            }

            return MimeType.Unknown; // Default MIME type representing unknown content types
        }
        public static string ToContentType(this MimeType mimeType)
        {
            foreach (var kvp in ContentTypeToMimeType)
            {
                if (kvp.Value == mimeType)
                {
                    return kvp.Key;
                }
            }

            return "application/octet-stream"; // Default content type for unknown MIME types
        }
        public static MimeType FromKeyword(string keyword)
        {
            if (keyword != null && KeywordToMimeType.TryGetValue(keyword, out MimeType mimeType))
            {
                return mimeType;
            }

            return MimeType.Unknown; // Default MIME type representing unknown content types
        }
        public static MimeType FromFileExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return MimeType.Unknown;

            // Normalize the extension to ensure it starts with a dot
            if (!extension.StartsWith('.'))
            {
                extension = "." + extension;
            }

            foreach (var kvp in MimeTypeToExtension)
            {
                if (string.Equals(kvp.Value, extension, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Key;
                }
            }

            return MimeType.Unknown; // Default for unrecognized extensions
        }
        public static bool IsTextContent(this MimeType mimeType)
        {
            // Define text-based MIME types
            var textMimeTypes = new HashSet<MimeType>
            {
                MimeType.TextPlain,
                MimeType.ApplicationJson,
                MimeType.ApplicationXml,
                MimeType.TextXml,
                MimeType.TextHtml,
                MimeType.TextCss,
                MimeType.TextJavascript,
                MimeType.ApplicationJavascript,
                MimeType.ApplicationXJavascript,
                MimeType.TextCsv
            };

            // Check if the MIME type is in the text-based set
            return textMimeTypes.Contains(mimeType);
        }
        public static bool IsBinaryContent(this MimeType mimeType)
        {
            // A MIME type is binary if it's not classified as text
            return !mimeType.IsTextContent();
        }
    }
}
