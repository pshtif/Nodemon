/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Reflection;
using Nodemon.Attributes;

namespace Nodemon
{
    public class NodeSort
    {
        static public int GroupSort(FieldInfo p_field1, FieldInfo p_field2)
        {
            TitledGroupAttribute attribute1 = p_field1.GetCustomAttribute<TitledGroupAttribute>();
            TitledGroupAttribute attribute2 = p_field2.GetCustomAttribute<TitledGroupAttribute>();
            if (attribute1 == null && attribute2 == null)
                return OrderSort(p_field1, p_field2);

            if (attribute1 != null && attribute2 == null)
                return 1;

            if (attribute1 == null && attribute2 != null)
                return -1;

            if (attribute1.Group == attribute2.Group)
                return OrderSort(p_field1, p_field2);

            if (attribute1.Order != attribute2.Order)
                return attribute1.Order.CompareTo(attribute2.Order);

            int groupCompare = attribute1.Group.CompareTo(attribute2.Group);
            if (groupCompare != 0) return groupCompare;
            return DeclarationOrder(p_field1, p_field2);
        }

        static public int OrderSort(FieldInfo p_field1, FieldInfo p_field2)
        {
            OrderAttribute attribute1 = p_field1.GetCustomAttribute<OrderAttribute>();
            OrderAttribute attribute2 = p_field2.GetCustomAttribute<OrderAttribute>();

            if (attribute1 == null && attribute2 == null)
                return DeclarationOrder(p_field1, p_field2);

            if (attribute1 != null && attribute2 == null)
                return -1;

            if (attribute1 == null && attribute2 != null)
                return 1;

            int orderCompare = attribute1.Order.CompareTo(attribute2.Order);
            return orderCompare != 0 ? orderCompare : DeclarationOrder(p_field1, p_field2);
        }

        // Stable tiebreaker so Array.Sort never reorders fields that compare as
        // "equal" under the group / order rules. Reordering equal fields breaks
        // [Dependency] / [DependencySingle] when the dependent field happens to
        // get shuffled before the field it depends on — Layout pass then reads
        // the OLD value of the dependency (SetValue hasn't run yet for that
        // pass) while Repaint reads NEW, the conditional row's draw state
        // differs across the same OnGUI cycle, and IMGUI's control-count
        // guard fires "Getting control N's position in a group with only N
        // controls".
        //
        // MetadataToken preserves declaration order for fields of the same
        // class (lower token = declared earlier) and for fields of different
        // classes in the same inheritance chain (base-class fields have
        // lower tokens than derived). That matches what users expect from
        // declaration order in source.
        static int DeclarationOrder(FieldInfo a, FieldInfo b)
        {
            int byToken = a.MetadataToken.CompareTo(b.MetadataToken);
            if (byToken != 0) return byToken;
            // Same token can happen across modules / dynamic assemblies in
            // edge cases — fall back to name for absolute determinism.
            return string.CompareOrdinal(a.Name, b.Name);
        }
    }
}