using System;

namespace RamPaged
{
    public class OrderByPropertyAddressAttribute : Attribute
    {
        private string _propertyAddress;

        public OrderByPropertyAddressAttribute(string propertyAddress)
        {
            _propertyAddress = propertyAddress;
        }

        public virtual string PropertyAddress
        {
            get { return _propertyAddress; }
        }
    }
}