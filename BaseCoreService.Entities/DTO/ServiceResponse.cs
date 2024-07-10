using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities.DTO
{
    public class ServiceResponse
    {
        public List<ValidateResult>? ValidateResults { get; set; }

        public bool IsSuccess { get; set; } = true;

        public int? ErrorCode { get; set; }

        /// <summary>
        /// Displayed message for user
        /// </summary>
        public string? UserMessage { get; set; }

        /// <summary>
        /// Message from system
        /// </summary>
        public string? SystemMessage { get; set; }
        
        /// <summary>
        /// Response data
        /// </summary>
        public object? Data { get; set; }

        public bool? GetLastData { get; set; }

        public DateTime? ServerTime { get; set; }



        public ServiceResponse OnSuccess(object data)
        {
            this.Data = data;
            return this;
        }

        public ServiceResponse OnException(ExceptionResponse data)
        {
            this.IsSuccess = false;
            this.Data = data.Data;
            this.SystemMessage = data.ExceptionMessage;
            //todo
            return this;
        }

        public ServiceResponse OnError(ErrorResponse data)
        {
            this.IsSuccess = false;
            this.Data = data.Data;
            this.SystemMessage = data.ErrorMessage;
            this.ErrorCode = data.ErrorCode;
            //todo
            return this;
        }
    }

    public class DataResponse
    {
        public object? Data { get; set; }

    }

    public class ErrorResponse: DataResponse 
    {
        public string? ErrorMessage { get; set; }
        public int ErrorCode { get; set; } = 99;
    }

    public class ExceptionResponse : DataResponse
    {
        public string? ExceptionMessage { get; set; }
        public int ErrorCode { get; set; } = 99;
    }
}
