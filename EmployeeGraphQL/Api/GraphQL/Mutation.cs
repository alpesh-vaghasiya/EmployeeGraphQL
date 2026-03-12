using HotChocolate.Authorization;

namespace Api.GraphQL;

[Authorize]
public class Mutation
{
    [GraphQLName("uploadFileAsync")]
    public async Task<string> UploadFileAsync(IFile file)
    {
        if (file == null)
            throw new GraphQLException(ErrorBuilder.New().SetMessage("File is required.").SetCode("VALIDATION_ERROR").Build());


        // ✅ Extension Validation
        if (System.IO.Path.GetExtension(file.Name).ToLower() != ".csv")
            throw new GraphQLException(ErrorBuilder.New().SetMessage("Only CSV files are allowed.").SetCode("VALIDATION_ERROR").Build());


        // ✅ Size Validation (5MB)
        const long maxSize = 5 * 1024 * 1024; // 5MB

        if (file.Length > maxSize)
            throw new GraphQLException(ErrorBuilder.New().SetMessage("File size must be less than 5MB.").SetCode("VALIDATION_ERROR").Build());

        int recordCount = 0;

        using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            // Skip header (if exists)
            await reader.ReadLineAsync();

            while (!reader.EndOfStream)
            {
                await reader.ReadLineAsync();
                recordCount++;
            }
        }

        // ✅ Record Limit Validation
        if (recordCount >= 200)
            throw new GraphQLException(ErrorBuilder.New().SetMessage("File contains too many records. Maximum allowed is 200.").SetCode("VALIDATION_ERROR").Build());

        // ✅ Save File
        var folder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        Directory.CreateDirectory(folder);

        var filePath = System.IO.Path.Combine(folder, file.Name);

        using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        return $"File uploaded successfully. Total records: {recordCount}";
    }
}