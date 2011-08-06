#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

namespace Lokad.Cqrs.Core
{
    /// <summary>
    /// Funqlets are a set of components provided as a package 
    /// to an existing container (like a module).
    /// </summary>
    public interface IFunqlet
    {
        /// <summary>
        /// Configure the given container with the 
        /// registrations provided by the funqlet.
        /// </summary>
        /// <param name="container">Container to register.</param>
        void Configure(Container container);
    }
}