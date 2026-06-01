using System.Collections.Generic;
using UnityEngine;

namespace ElephantSDK
{
    public class HyperlinkData
    {
        public string mask;
        public string text;
        public string url;

        public HyperlinkData(string mask, string text, string url)
        {
            this.mask = mask;
            this.text = text;
            this.url = url;
        }
    }
    
    public static class HyperlinkUtils
    {
        public const string PRIVACY_MASK = "{{privacy}}";
        public const string TERMS_MASK = "{{terms}}";
        public const string DATA_REQUEST_MASK = "{{datarequest}}";
        
        public static string ProcessHyperlinks(string content, List<HyperlinkData> hyperlinks)
        {
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogWarning("[HyperlinkUtils] Content is null or empty");
                return content;
            }

            foreach (var hyperlink in hyperlinks)
            {
                if (string.IsNullOrEmpty(hyperlink.mask))
                {
                    Debug.LogWarning("[HyperlinkUtils] Hyperlink mask is null or empty, skipping");
                    continue;
                }

                if (string.IsNullOrEmpty(hyperlink.url))
                {
                    Debug.LogWarning($"[HyperlinkUtils] URL is empty for mask '{hyperlink.mask}', skipping");
                    continue;
                }

                string linkHtml = $"<link=\"{hyperlink.url}\"><color=#4A9EFF><u>{hyperlink.text}</u></color></link>";
                
                content = content.Replace(hyperlink.mask, linkHtml);
                
                Debug.Log($"[HyperlinkUtils] Replaced '{hyperlink.mask}' with link to: {hyperlink.url}");
            }

            return content;
        }
        
        public static string CleanText(string rawText)
        {
            var cleaned = rawText.Replace("\t", "");
    
            var lines = cleaned.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }
            cleaned = string.Join("\n", lines);
    
            return cleaned;
        }
        
        public static void OpenURL(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning("[HyperlinkUtils] Attempted to open empty URL");
                return;
            }

            Debug.Log($"[HyperlinkUtils] Opening URL: {url}");
            Application.OpenURL(url);
        }
    }
}