using System;
using System.Collections.Generic;
using System.Text;

namespace ClockApp.Infrastructure.TimeOn.Dtos
{
    public class HourListDto
    {
        public List<HourDto> Hours { get; set; } = new();
    }
}
