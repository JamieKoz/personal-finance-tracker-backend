using Google.Cloud.Storage.V1;
using Google;

namespace PersonalFinanceTracker.Persistence
{
    public class SqliteBackupService
    {
        private readonly StorageClient? _storageClient;
        private readonly string _bucketName = "personal-finance-tracker-sqlite-backups";
        private readonly string _dbPath = "transactions.db";
        private readonly string _backupObjectName = "transactions-backup.db";
        private readonly bool _isEnabled;

        public SqliteBackupService()
        {
            try
            {
                _storageClient = StorageClient.Create();
                _isEnabled = true;
            }
            catch
            {
                // Running locally without credentials, disable backup
                _isEnabled = false;
                Console.WriteLine("Cloud Storage backup disabled (local development)");
            }
        }

        public async Task BackupDatabase()
        {
            if (!_isEnabled || _storageClient == null) return;

            if (File.Exists(_dbPath))
            {
                using var fileStream = File.OpenRead(_dbPath);
                await _storageClient.UploadObjectAsync(_bucketName, _backupObjectName, null, fileStream);
            }
        }

        public async Task RestoreLatestBackup()
        {
            if (!_isEnabled || _storageClient == null) return;

            try
            {
                if (!File.Exists(_dbPath))
                {
                    using var fileStream = File.Create(_dbPath);
                    await _storageClient.DownloadObjectAsync(_bucketName, _backupObjectName, fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not restore backup: {ex.Message}");
            }
        }
    }
}
