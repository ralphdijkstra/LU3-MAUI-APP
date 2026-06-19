using System;
using System.Collections.Generic;
using System.Text;

namespace ClockApp.Infrastructure.Http
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; init; }

        public T? Data { get; init; }

        public string? ErrorMessage { get; init; }

        public int StatusCode { get; init; }
    }
}
