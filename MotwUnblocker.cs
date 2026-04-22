
namespace Remove_Web_Properties
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class MotwUnblocker
    {
        // Optional: plug in a logger from your form
        public static Action<string> Log { get; set; }

        public static int UnblockDirectory(string folder, bool recursive)
        {
            int count = 0;

            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                Log?.Invoke($"[WARN] Folder not found: {folder}");
                return 0;
            }

            try
            {
                // Files in this folder
                foreach (var file in Directory.GetFiles(folder))
                {
                    if (TryUnblockFile(file))
                    {
                        count++;
                        Log?.Invoke($"[OK] Unblocked: {file}");
                    }
                }

                // Recurse into subfolders
                if (recursive)
                {
                    foreach (var dir in Directory.GetDirectories(folder))
                    {
                        count += UnblockDirectory(dir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.Invoke($"[ERROR] UnblockDirectory failed on {folder}: {ex.Message}");
            }

            return count;
        }

        public static bool TryUnblockFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                // This is the *only* thing that controls the Unblock checkbox
                string adsPath = filePath + ":Zone.Identifier";

                // Just try to delete it; Exists() lies for ADS
                File.Delete(adsPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Convenience: unblock a single file directly
        public static bool UnblockSingle(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log?.Invoke($"[WARN] File not found: {filePath}");
                return false;
            }

            bool ok = TryUnblockFile(filePath);
            Log?.Invoke(ok ? $"[OK] Unblocked: {filePath}" : $"[SKIP] Not blocked or cannot unblock: {filePath}");
            return ok;
        }
    }
}
//Usage Example:
//int count = MotwUnblocker.UnblockDirectory(@"C:\Downloads", true);
//Console.WriteLine($"Unblocked {count} files.")
//}
