using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Dtos
{
    public class ErrorDto
    {
        public List<string> Errors { get; private set; } = new List<string>();
        public bool IsShow { get; private set; }

        // Tek Hata Alırsak Bu Kod Yeterli Olur
        public ErrorDto(string error, bool isShow)
        {
            Errors.Add(error);
            IsShow = isShow;
        }

        // Birden Fazla Hatalarda İse Bu Kod Yeterli
        public ErrorDto(List<string> errors,bool isShow)
        {
            Errors=errors;
            IsShow = isShow;
        }
    }
}
