using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedLibrary.Exceptions
{
    public static class CustomExceptionHandler
    {
        public static void UseCustomException(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseExceptionHandler(config =>
            {
                config.Run(async context =>
                {
                    context.Response.StatusCode = 500; // Statüs kodu dönüyoruz.
                    context.Response.ContentType = "application/json"; // Json formatında tipini belirliyoruz.
                    var errorFeature = context.Features.Get<IExceptionHandlerFeature>(); // İnterface üzerinden hata yakalıyoruz.

                    if (errorFeature != null)
                    {
                        var exception = errorFeature.Error; // Hatayı yakalıyorız.
                        ErrorDto errorDto = null; // Null set ediyoruz.
                        if (exception is CustomException)
                        {
                            errorDto = new ErrorDto(exception.Message, true);
                        }
                        else
                        {
                            errorDto = new ErrorDto(exception.Message, false);
                        }
                        var response = Response<NoDataDto>.Fail(errorDto, 500);
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    }
                });
            });
        }
    }
}
