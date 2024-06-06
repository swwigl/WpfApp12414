//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WpfApp1
{
    using System;
    using System.Collections.Generic;
    
    public partial class Demands
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Demands()
        {
            this.Deals = new HashSet<Deals>();
        }
    
        public int ID { get; set; }
        public string Adress_City { get; set; }
        public string Adress_Street { get; set; }
        public string Adress_House { get; set; }
        public Nullable<int> Adress_Number { get; set; }
        public Nullable<double> Min_Price { get; set; }
        public Nullable<double> Max_Price { get; set; }
        public Nullable<int> FK_AgentID { get; set; }
        public Nullable<int> FK_ClientID { get; set; }
        public Nullable<double> MinArea { get; set; }
        public Nullable<double> MaxArea { get; set; }
        public Nullable<int> MinRooms { get; set; }
        public Nullable<int> MaxRooms { get; set; }
        public Nullable<int> MinFloor { get; set; }
        public Nullable<int> MaxFloor { get; set; }
        public Nullable<int> FK_Type_Object_ID { get; set; }
    
        public virtual Agents Agents { get; set; }
        public virtual Clients Clients { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Deals> Deals { get; set; }
        public virtual Type_Object Type_Object { get; set; }
    }
}
