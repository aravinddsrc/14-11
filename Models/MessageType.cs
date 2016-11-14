using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSRCManagementSystem.Models
{

    public enum commMessageType
    {
        Message=1,
        Yesorno=2
    };

    public class MessageType
    {
        public int typeId { get; set; }

        public string description { get; set; }

        public bool isSelected { get; set; }
    }
}
