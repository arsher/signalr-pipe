﻿using System;
using System.Security.Cryptography;
using System.Text;

namespace SignalR.Pipes.Common
{
    //this is almost identical to what wcf does
    public static class PipeUri
    {
        public const string NamedPipeScheme = "signalr.pipe";

        public static string GetAcceptorName(Uri uri)
        {
            var path = GetUriPath(uri);
            var host = uri.Host;

            return GetAcceptorName(host, path);
        }

        public static void Validate(Uri uri)
        {
            if(uri.Scheme != NamedPipeScheme)
            {
                throw new ArgumentException(nameof(uri));
            }
        }

        private static string GetAcceptorName(string hostName, string path)
        {
            var builder = new StringBuilder();
            builder.Append(NamedPipeScheme);
            builder.Append("://");
            builder.Append(hostName.ToUpperInvariant());
            builder.Append(path);
            var canonicalName = builder.ToString();

            var canonicalBytes = Encoding.UTF8.GetBytes(canonicalName);
            byte[] hashedBytes;
            string separator;

            if (canonicalBytes.Length >= 128)
            {
                using (HashAlgorithm hash = new SHA1Managed())
                {
                    hashedBytes = hash.ComputeHash(canonicalBytes);
                }
                separator = "_H";
            }
            else
            {
                hashedBytes = canonicalBytes;
                separator = "_E";
            }

            builder = new StringBuilder();
            builder.Append(NamedPipeScheme);
            builder.Append(separator);
            builder.Append(Convert.ToBase64String(hashedBytes));

            return builder.ToString();
        }

        private static string GetUriPath(Uri uri)
        {
            var path = uri.LocalPath.ToUpperInvariant();
            if (!path.EndsWith("/", StringComparison.Ordinal))
            {
                path = path + "/";
            }
            return path;
        }
    }
}
