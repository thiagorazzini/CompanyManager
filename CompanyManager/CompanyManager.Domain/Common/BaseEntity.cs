using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyManager.Domain.Common
{
    public abstract class BaseEntity : IEquatable<BaseEntity>
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; protected set; }

        protected void UpdateModifiedAt() => UpdatedAt = DateTime.UtcNow;

        public override bool Equals(object? obj) => obj is BaseEntity other && other.Id == Id;
        public bool Equals(BaseEntity? other) => other is not null && other.Id == Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}
