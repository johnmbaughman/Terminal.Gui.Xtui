using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Terminal.Gui.Xaml.Model;
    /// <summary>
    /// Represents event metadata and handler binding for XAML elements.
    /// </summary>
    public class EventInfo
    {
        /// <summary>
        /// The name of the event.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The delegate type for the event handler.
        /// </summary>
        public Type HandlerType { get; }

        /// <summary>
        /// The underlying reflection EventInfo.
        /// </summary>
        public System.Reflection.EventInfo ReflectionInfo { get; }

        /// <summary>
        /// Indicates if the event handler is async.
        /// </summary>
        public bool IsAsyncHandler { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventInfo"/> class.
        /// </summary>
        public EventInfo(string name, Type handlerType, System.Reflection.EventInfo reflectionInfo, bool isAsyncHandler = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
            ReflectionInfo = reflectionInfo ?? throw new ArgumentNullException(nameof(reflectionInfo));
            IsAsyncHandler = isAsyncHandler;
        }

        /// <summary>
        /// Registers an event handler for the event.
        /// </summary>
        public void RegisterHandler(object target, Delegate handler)
        {
            ReflectionInfo.AddEventHandler(target, handler);
        }

        /// <summary>
        /// Validates the event handler signature.
        /// </summary>
        public bool ValidateHandler(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var delegateParams = HandlerType.GetMethod("Invoke")?.GetParameters();
            if (delegateParams == null || parameters.Length != delegateParams.Length)
            {
                return false;
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != delegateParams[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Dispatches the event to the handler, supporting async if needed.
        /// </summary>
        public async Task DispatchAsync(object sender, object[] args, Delegate handler)
        {
            if (IsAsyncHandler && handler is Func<object, object[], Task> asyncHandler)
            {
                await asyncHandler(sender, args);
            }
            else
            {
                handler.DynamicInvoke(args.Prepend(sender).ToArray());
            }
        }
    }
