//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LoginTest
{
    using System;
    using System.Collections.Generic;
    
    public partial class AUDITORIAS
    {
        public short IDAUDITORIA { get; set; }
        public Nullable<byte> IDUSUARIO { get; set; }
        public string ACCION { get; set; }
        public Nullable<System.DateTime> TIMESTAMP { get; set; }
    
        public virtual USUARIOS USUARIOS { get; set; }
    }
}
