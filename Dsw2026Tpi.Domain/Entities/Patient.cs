using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Patient: EntityBase
    {
        public Guid UserId { get; private set; }
        public string FullName { get; private set; }
        public long Dni { get; private set; }

        #region Constructor for EF
#pragma warning disable CS8618
        private Patient()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Patient ( Guid userId, long dni, string fullName = "", Guid? id = null) : base(id)
        {
            UserId = userId;
            Dni = dni;
            FullName = fullName;
        }
    }
}