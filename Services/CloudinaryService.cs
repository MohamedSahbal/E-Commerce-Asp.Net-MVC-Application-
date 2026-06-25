using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ECommerceApp.Services;

public class CloudinaryService
{
    private readonly Cloudinary? _cloudinary;
    private readonly string _wwwroot;
    private readonly bool _isConfigured;

    public CloudinaryService(IConfiguration config, IWebHostEnvironment env)
    {
        _wwwroot = env.WebRootPath;

        var cloudName = config["Cloudinary:CloudName"];
        var apiKey    = config["Cloudinary:ApiKey"];
        var apiSecret = config["Cloudinary:ApiSecret"];

        _isConfigured = !string.IsNullOrEmpty(cloudName)
            && cloudName != "your-cloud-name"
            && !string.IsNullOrEmpty(apiKey)
            && apiKey != "your-api-key";

        if (_isConfigured)
        {
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }
    }

    public async Task<(string Url, string PublicId)> UploadImageAsync(
        IFormFile file, string folder = "ecommerce/products")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file");
        if (_isConfigured)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File           = new FileDescription(file.FileName, stream),
                Folder         = folder,
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };
            var result = await _cloudinary!.UploadAsync(uploadParams);
            if (result.Error is not null)
                throw new Exception(result.Error.Message);
            return (result.SecureUrl.ToString(), result.PublicId);
        }

        // Local fallback: save under wwwroot/uploads/<folder>/
        var saveDir = Path.Combine(_wwwroot, "uploads",
            folder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(saveDir);

        var ext      = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(saveDir, fileName);

        await using var fs = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(fs);

        var relUrl = $"/uploads/{folder}/{fileName}";
        return (relUrl, relUrl);
    }

    public async Task DeleteImageAsync(string publicId)
    {
        if (publicId.StartsWith("/uploads/"))
        {
            var fullPath = Path.Combine(_wwwroot,
                publicId.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            return;
        }

        if (_cloudinary is not null)
        {
            var deleteParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}
