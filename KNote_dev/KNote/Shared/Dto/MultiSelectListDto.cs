using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Shared.Dto
{
    public class MultiSelectListDto
    {
        public MultiSelectListDto(bool selected, string key, string value)
        {
            Selected = selected;
            Key = key;
            Value = value;
        }

        public bool Selected { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
