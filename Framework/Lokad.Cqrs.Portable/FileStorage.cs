﻿#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;
using Lokad.Cqrs.Feature.TapeStorage;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Lokad.Cqrs
{
    public static class FileStorage
    {
        /// <summary>
        /// Creates the simplified nuclear storage wrapper around Atomic storage, using the default
        /// storage configuration and atomic strategy.
        /// </summary>
        /// <param name="storageFolder">The storage folder.</param>
        /// <returns>
        /// new instance of the nuclear storage
        /// </returns>
        public static NuclearStorage CreateNuclear(string storageFolder)
        {
            return CreateNuclear(storageFolder, b => { });
        }

        /// <summary>
        /// Creates the simplified nuclear storage wrapper around Atomic storage.
        /// </summary>
        /// <param name="storageFolder">The storage folder.</param>
        /// <param name="configStrategy">The config strategy.</param>
        /// <returns></returns>
        public static NuclearStorage CreateNuclear(string storageFolder,
            Action<DefaultAtomicStorageStrategyBuilder> configStrategy)
        {
            var strategyBuilder = new DefaultAtomicStorageStrategyBuilder();
            configStrategy(strategyBuilder);
            var strategy = strategyBuilder.Build();
            return CreateNuclear(storageFolder, strategy);
        }


        /// <summary>
        /// Creates the simplified nuclear storage wrapper around Atomic storage.
        /// </summary>
        /// <param name="storageFolder">The storage folder.</param>
        /// <param name="strategy">The atomic storage strategy.</param>
        /// <returns></returns>
        public static NuclearStorage CreateNuclear(string storageFolder, IAtomicStorageStrategy strategy)
        {
            var factory = new FileAtomicStorageFactory(storageFolder, strategy);
            factory.Initialize();
            return new NuclearStorage(factory);
        }

        public static IStreamingRoot CreateStreaming(string storageFolder)
        {
            var container = new FileStreamingContainer(storageFolder);
            container.Create();
            return container;
        }

        /// <summary>
        /// Creates and initializes the tape storage in the provided folder.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns></returns>
        public static FileTapeStorageFactory CreateTape(string folderPath)
        {
            var factory = new FileTapeStorageFactory(folderPath);
            factory.InitializeForWriting();
            return factory;
        }


        public static FileStorageConfig CreateConfig(string fullPath, string optionalName = null)
        {
            var folder = new DirectoryInfo(fullPath);
            return new FileStorageConfig(folder, optionalName ?? folder.Name);
        }

        public static FileStorageConfig CreateConfig(DirectoryInfo info, string optionalName = null)
        {
            return new FileStorageConfig(info, optionalName ?? info.Name);
        }
    }
}