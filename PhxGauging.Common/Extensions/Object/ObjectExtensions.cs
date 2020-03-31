namespace PhxGauging.Common.Extensions.Object
{
    public static class ObjectExtensions
    {

        /// <summary>
        /// Checks if the object is null.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }
    }

}
