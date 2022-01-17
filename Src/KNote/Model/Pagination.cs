using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model
{
    public class PaginationContext
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public long TotalCount { get; set; }
        public int TotalPages 
        {
            get { return (int) (Math.Ceiling((double)TotalCount / PageSize)); }
            set { }
        }
        

        public PageIdentifier PageIdentifier
        {
            get { return new PageIdentifier { PageNumber = CurrentPage, PageSize = PageSize }; }
        }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    public class PageIdentifier
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 99999;
        public int Offset => (PageNumber - 1) * PageSize;
    }
}
