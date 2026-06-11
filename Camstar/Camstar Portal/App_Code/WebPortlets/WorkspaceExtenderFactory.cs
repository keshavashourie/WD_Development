using System;
using System.Linq;

namespace Camstar.WebPortal.WebPortlets
{
    /// <summary>
    /// There is code in the core workspace that needs to modified sometimes to 
    /// for industry solutions projects. The solution thus far has been to copy the 
    /// original .cs file, make modifications, and then replace the original file during
    /// the installation. This obviously has several drawbacks. The WorkspaceExtenderFactory
    /// class is a solution to that problem.
    /// 
    /// The WorkspaceExtenderFactory uses .NET Reflection, custom attributes and OOP to 
    /// create in the core code, access points where a workspace can add custom logic into
    /// the original class and yet have those changes remain seperate from the core code base
    //  and install.
    /// </summary>
    /// <remarks>
    /// The steps to create a workspace extender for a class is as follows:
    ///     1. Create an abstract class inside the WorkspaceExtenders.cs file. The class should define all
    ///         of methods that the core code will call to get the custom workspace customizations. The class
    ///         should be suffixed with the word "Extender" as in MyClassExtender. 
    ///  
    ///     2. Create a new class to contain the business logic for the workspace. It will inherit from the base
    ///         class created in step 1 above. The class should have a suffix of "Extension" as in MyClassExtension.
    ///         
    ///     3. Add a "WorkspaceAttribute" attribute to the class. This will be used by the factory to determine
    ///         the highest workspace version of an the extensions. That is to say this supports inheritence.
    ///         
    ///     4. In the core class get an instance of the extension class to run logic for by calling the WorkspaceExtenderFactory.GetImplementation()
    ///         method.
    ///         
    ///     5. Create your logic in the core code that will call the methods of the extender class you just created.
    ///     
    /// Adding code in a workspace that is higher than an existing extension:
    ///     1. Follow the steps above, except the new class will inherit from the class definition of the lower numbered workspace. It is up the
    ///         the developer to reconcile the base calls with the new code being added.
    ///         
    /// </remarks>
    /// <example>
    ///     bool hasCustomWorkspaceLogic = false;
    ///     workspaceCustomLogic = Camstar.WebPortal.WebPortlets.WorkspaceExtenderFactory.GetImplementation<JtToBodConvertHandlerExtender>();
    ///     if (workspaceCustomLogic != null)
    ///     {
    ///          docInfo = workspaceCustomLogic.GetDocInfo(docRef, context, out result);
    ///          hasCustomWorkspaceLogic = true;
    ///     }
    /// </example>
    public class WorkspaceExtenderFactory
    {
        /// <summary>
        /// Get an instance of an "extender" class in the highest available workspace implementation.
        /// </summary>
        /// <typeparam name="T">The extender abstract class type from WorkspaceExtenders.cs</typeparam>
        /// <returns>An instance of the extension class if found. Otherwise null.</returns>
        public static T GetImplementation<T>()
        {
            Type typeToLoad = typeof(T);
            T result = default(T);
            Type classToInstantiate = typeToLoad;
            int highestWorkspaceNumber = 0;
            bool classMatchFound = false;

            if (!typeToLoad.IsClass)
                throw new NotSupportedException("WorkspaceExtenderFactory only accepts classes as the type <T> value");
            else if (!typeToLoad.IsAbstract)
                throw new NotSupportedException("WorkspaceExtenderFactory only accepts abstract classes as the type <T> value");

            // Now look to see if there are any class implementations to load
            foreach (Type classFound in AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => typeToLoad.IsAssignableFrom(p) && p.IsAbstract == false))
            {
                // Get the class's WorkspaceExtenderAttribute attribute
                object[] attr = classFound.GetCustomAttributes(typeof(WorkspaceExtenderAttribute), false);
                if (attr.Length == 0)
                    throw new Exception($"No WorkspaceExtenderAttribute found for the {classFound.Name}");

                WorkspaceExtenderAttribute workspace = (WorkspaceExtenderAttribute)attr[0];

                // Now is this the highest workspace number found thusfar? If so, store the type value to
                // potentially be instantiated later.
                if (workspace.WorkspaceNumber > highestWorkspaceNumber)
                {
                    highestWorkspaceNumber = workspace.WorkspaceNumber;
                    classMatchFound = true;
                    classToInstantiate = classFound;
                }
            }

            if (classMatchFound)
                result = (T)Activator.CreateInstance(classToInstantiate);

            return result;
        }
    }

    /// <summary>
    /// The Workspace enum can used for one of the WorkspaceAttribute constructors. If
    /// this enum is ever used for a purpose other than with the WorkspaceAttribute
    /// then it should be moved out of this .cs file.
    /// </summary>
    public enum WorkspaceCode
    {
        Industry = 15,
        Semiconductor = 40,
        Electronics = 60
    }

    /// <summary>
    /// This attribute is used with the <see cref="WorkspaceExtenderFactory"/> class.
    /// When a developer creates a new extension class, they will mark the class with 
    /// attribute. This will allow the factory to sort results by workpace number
    /// and choose the highest workspace value's class and instantiate it.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class WorkspaceExtenderAttribute : System.Attribute
    {
        public int WorkspaceNumber { get; set; }

        private WorkspaceExtenderAttribute() { }

        public WorkspaceExtenderAttribute(int workspace)
        {
            WorkspaceNumber = workspace;
        }

        public WorkspaceExtenderAttribute(WorkspaceCode workspace)
        {
            WorkspaceNumber = (int)workspace;
        }
    }

}