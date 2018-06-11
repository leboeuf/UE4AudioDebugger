using System;

namespace UE4AudioDebugger.Models
{
    public class UActor
    {
        public string Name { get; set; }
        public Vector3 Location { get; set; }
        public Vector3 Size { get; set; }
        public Vector3 Origin { get; set; }
        public bool IsSelected { get; set; }
    }
}
