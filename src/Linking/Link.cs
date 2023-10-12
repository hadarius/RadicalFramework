using NLog.Targets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Radical.Uniques;

namespace Radical.Linking
{  
    public class Link<TSource, TTarget> : Link
    {
        public Link(string suffix = null)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            Set(sourceType, targetType, suffix);
        }       
    }

    [DataContract]
    public class Link : UniqueObject, ILink
    {
        private string _suffix;

        public Link()
        {
        }

        public Link(object source, object target, string suffix = null)
        {           
            Set(source, target, suffix);
        }
        public Link(Type source, Type target, string suffix = null)
        {
            Set(source, target, suffix);
        }

        public Link Set(Type source, Type target, string suffix = null)
        {
            var sourceType = source;
            var targetType = target;
            SourceType = sourceType.FullName;
            TargetType = targetType.FullName;
            Label = sourceType.Name + "To" + targetType.Name + suffix;
            TypeKey = (int)typeof(Link<,>).MakeGenericType(sourceType, targetType).UniqueKey32();
            if (!Linker.Types.ContainsKey(this.GetType()))
                Linker.Types.TryAdd(this.GetType());
            return this;
        }
        public Link Set(object source, object target, string suffix = null)
        {
            _suffix = suffix;
            var sourceType = source.GetType();
            var targetType = target.GetType();
            SourceType = sourceType.FullName;
            TargetType = targetType.FullName;
            Label = sourceType.Name + "To" + targetType.Name + _suffix;
            TypeKey = (int)typeof(Link<,>).MakeGenericType(sourceType, targetType).UniqueKey32();
            SourceId = (long)source.UniqueKey();
            TargetId = (long)target.UniqueKey();
            Id = (long)new long[] { SourceId, TargetId }.UniqueKey();
            Join(source, target);   
                     
            return this;
        }

        public Link Join(object source, object target)
        {
            SourceId = (long)source.UniqueKey();
            TargetId = (long)target.UniqueKey();
            Id = (long)new long[] { SourceId, TargetId }.UniqueKey();

            Linker.Links.Memorize(this);

            if (!Linker.Types.ContainsKey(this.GetType()))
                Linker.Types.TryAdd(this.GetType());

            return this;
        } 

        public Link Reversed()
        {
            var link = new Link(Type.GetType(TargetType), Type.GetType(SourceType), _suffix);
            link.SourceId = this.TargetId;
            link.TargetId = this.SourceId;
            link.Id = (long)new long[] { link.SourceId, link.TargetId }.UniqueKey();

            Linker.Links.Memorize(link);

            if (!Linker.Types.ContainsKey(link.GetType()))
                Linker.Types.TryAdd(link.GetType());

            return link;
        }
    }
}
