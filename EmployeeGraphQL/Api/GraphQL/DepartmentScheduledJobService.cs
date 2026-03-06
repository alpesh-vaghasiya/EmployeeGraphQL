using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using EmployeeGraphQL.Infrastructure;   // <-- Add this (namespace of RedisStreamProducer)

namespace EmployeeGraphQL.Application.Services
{
    public class DepartmentScheduledJobService
    {
        private readonly RedisStreamProducer _redisProducer;
        private readonly ILogger<DepartmentScheduledJobService> _logger;

        public DepartmentScheduledJobService(
            RedisStreamProducer redisProducer,
            ILogger<DepartmentScheduledJobService> logger)
        {
            _redisProducer = redisProducer;
            _logger = logger;
        }

        public async Task PublishDepartmentJob()
        {
            var jobId = Guid.NewGuid().ToString();

            _logger.LogInformation("⏰ Scheduled Department Import Job triggered. JobId={JobId}", jobId);

            // 👉 Get latest uploaded CSV url from DB  
            string fileUrl = await GetLatestDepartmentCsvUrl();   // you will implement this

            if (string.IsNullOrEmpty(fileUrl))
            {
                _logger.LogWarning("⚠ No CSV file found to process. Skipping job.");
                return;
            }

            // await _redisProducer.PublishJobAsync(jobId, fileUrl);

            _logger.LogInformation("📨 Job pushed to Redis Stream. JobId={JobId}", jobId);
        }

        private async Task<string> GetLatestDepartmentCsvUrl()
        {
            // TODO: Replace with DB lookup
            return "http://dev.projecttree.in/alm-api/v1.0/FileManage/af5d4a5b-0696-405b-87ee-0d7a50122d01";
        }
    }
}