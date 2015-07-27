using Autofac;
using MSOE.MediaComplete.Search.Lucene;
using MSOE.MediaComplete.Search.Music;

namespace MSOE.MediaComplete.Search
{
    /// <summary>
    /// Manages dependency injection for searching functions
    /// </summary>
    public static class Dependency
    {
        private static IContainer _afContainer;

        /// <summary>
        /// Initializes the dependency chains in the application. 
        /// This must be called before attempting to resolve anything.
        /// </summary>
        public static void Build()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<LuceneIndex>().As<IIndex>();
            builder.RegisterType<FileMusicSearcher>().As<IMusicIndex>();
            _afContainer = builder.Build();
        }

        /// <summary>
        /// Used to get an instance of a dependency that will/should persist for the lifetime of the application. e.g. single instance classes
        /// </summary>
        /// <typeparam name="T">The type of dependency to resolve</typeparam>
        /// <returns>
        /// A globally resolved scope
        /// </returns>
        public static T Resolve<T>()
        {
            if (_afContainer == null) Build();
            return _afContainer.Resolve<T>();
        }
        /// <summary>
        /// Used to get an instance of a dependency with a lifetime. i.e. something that will end during the lifetime of the application
        /// </summary>
        /// <returns>A scope from which to resolve dependencies.</returns>
        public static ILifetimeScope BeginLifetimeScope()
        {
            return _afContainer.BeginLifetimeScope();
        }
    }
}
