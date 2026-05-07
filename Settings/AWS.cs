using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Capstone_2_BE.Settings
{
    public class AWS
    {
        private readonly string _bucketName;
        private readonly IAmazonS3 _s3Client;

        public AWS(IConfiguration configuration)
        {
            _bucketName = configuration["AWS:BucketName"]
                ?? throw new InvalidOperationException("Missing configuration: AWS:BucketName");
            var accessKey = configuration["AWS:AccessKey"]
                ?? throw new InvalidOperationException("Missing configuration: AWS:AccessKey");
            var secretKey = configuration["AWS:SecretKey"]
                ?? throw new InvalidOperationException("Missing configuration: AWS:SecretKey");
            var regionName = configuration["AWS:Region"]
                ?? throw new InvalidOperationException("Missing configuration: AWS:Region");
            _s3Client = new AmazonS3Client(
                accessKey,
                secretKey,
                RegionEndpoint.GetBySystemName(regionName)
            );
        }

        public async Task<bool> DeleteImage(string key)
        {
            var deleteObject = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };
            try
            {
                var response = await _s3Client.DeleteObjectAsync(deleteObject);
                return true; // xoá thành công
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting file: " + ex.Message);
                return false;
            }
        }

        public async Task<string?> UploadProfile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0) return null;

                string key = $"profile/{Guid.NewGuid()}_{file.FileName}";
                using var stream = file.OpenReadStream();

                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = file.ContentType
                };

                await _s3Client.PutObjectAsync(request);
                return key;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AWS] Upload failed (profile): {ex}");
                return null;
            }
        }

        public async Task<string?> UploadVideoOrder(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0) return null;
                string key = $"Video/{Guid.NewGuid()}_{file.FileName}";

                using var source = file.OpenReadStream();
                using var stream = new MemoryStream();
                await source.CopyToAsync(stream);
                stream.Position = 0;

                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = file.ContentType
                };

                await _s3Client.PutObjectAsync(request);
                return key;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AWS] Upload failed (order video): {ex}");
                return null;
            }
        }
        public async Task<string?> UploadImageOrder(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0) return null;
                string key = $"Image/{Guid.NewGuid()}_{file.FileName}";

                using var source = file.OpenReadStream();
                using var stream = new MemoryStream();
                await source.CopyToAsync(stream);
                stream.Position = 0;

                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = file.ContentType
                };

                await _s3Client.PutObjectAsync(request);
                return key;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AWS] Upload failed (order image): {ex}");
                return null;
            }
        }

        public async Task<string?> UploadChatImage(IFormFile file)
        {
            return await UploadFile("Chat/Image", file);
        }

        public async Task<string?> UploadChatVideo(IFormFile file)
        {
            return await UploadFile("Chat/Video", file);
        }

        private async Task<string?> UploadFile(string folder, IFormFile file)
        {
            try
            {
                if (file == null || file.Length <= 0 || string.IsNullOrWhiteSpace(folder))
                {
                    return null;
                }

                var safeFileName = string.IsNullOrWhiteSpace(file.FileName) ? "upload.bin" : file.FileName;
                var safeContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
                string key = $"{folder}/{Guid.NewGuid()}_{safeFileName}";

                using var source = file.OpenReadStream();
                using var stream = new MemoryStream();
                await source.CopyToAsync(stream);
                stream.Position = 0;

                Console.WriteLine($"[AWS] Start upload: {key}, size={file.Length} bytes, contentType={safeContentType}");

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _bucketName ?? string.Empty,
                    Key = key,
                    InputStream = stream,
                    ContentType = safeContentType
                };

                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(uploadRequest);
                Console.WriteLine($"[AWS] Upload success: {key}");
                return key;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AWS] Upload failed ({folder}): {ex}");
                return null;
            }
        }
        public Task<string?> ReadImage(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Task.FromResult<string?>(null);
            }

            return Task.FromResult<string?>($"https://{_bucketName}.s3.ap-southeast-2.amazonaws.com/{key}");
        }
    }
}