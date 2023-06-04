using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface IQueueTaskService
    {
        List<QueueTask> GetAllQueue(QueueStatus status, string job, int pageSize = 10, bool isOrderDesc = true);

        Task<GenericResult> ProcessVerificationTransaction();

    }
}
