namespace Ql_NhaTro_jun.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public string Error { get; set; }

        public static ApiResponse<(T1, T2)> CreateSuccess<T1, T2>(string message, T1 data1, T2 data2)
        {
            return new ApiResponse<(T1, T2)>
            {
                Success = true,
                Message = message,
                Data = (data1, data2)
            };
        }
        public static ApiResponse<T> CreateSuccess(string message, T data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }


        public static ApiResponse<T> CreateError(string message, string error = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = error
            };
        }
    }
}
