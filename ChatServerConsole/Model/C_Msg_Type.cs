//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ChatServerConsole.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class C_Msg_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public C_Msg_Type()
        {
            this.C_Group_Msg_Status = new HashSet<C_Group_Msg_Status>();
            this.C_Multi_Msg = new HashSet<C_Multi_Msg>();
            this.C_Single_Msg = new HashSet<C_Single_Msg>();
        }
    
        public int ID { get; set; }
        public string Msg_Type_ID { get; set; }
        public string Msg_Type_Description { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C_Group_Msg_Status> C_Group_Msg_Status { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C_Multi_Msg> C_Multi_Msg { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C_Single_Msg> C_Single_Msg { get; set; }
    }
}
