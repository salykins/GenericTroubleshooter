using System;
namespace GenericTroubleshooting.DataClasses
{
    public class CustomObject
    {
        public long ObjId { get; set; }
        public long tenantId { get; set; }
        public string ObjType { get; set; }
        public string ObjName { get; set; }
        public string ObjNameAPI { get; set; }
        public string ObjDesc { get; set; }
        public short Active { get; set; }
    }
}
