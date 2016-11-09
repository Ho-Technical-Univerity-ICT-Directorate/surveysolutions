﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    internal class QuestionnaireAssemblyFileAccessor : IQuestionnaireAssemblyFileAccessor
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string pathToStore;

        private static ILogger Logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<QuestionnaireAssemblyFileAccessor>();

        public QuestionnaireAssemblyFileAccessor(IFileSystemAccessor fileSystemAccessor, string folderPath, string assemblyDirectoryName)
        {
            this.fileSystemAccessor = fileSystemAccessor;

            this.pathToStore = fileSystemAccessor.CombinePath(folderPath, assemblyDirectoryName);
            if (!fileSystemAccessor.IsDirectoryExists(this.pathToStore))
                fileSystemAccessor.CreateDirectory(this.pathToStore);
        }

        public string GetFullPathToAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            return this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64)
        {
            this.StoreAssembly(questionnaireId, questionnaireVersion, Convert.FromBase64String(assemblyAsBase64));
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, byte[] assembly)
        {
            if (assembly.Length == 0)
            {
                throw new ArgumentException($"Assembly file is empty. Cannot be saved. Questionnaire: {new QuestionnaireIdentity(questionnaireId, questionnaireVersion)}", 
                    nameof(assembly));
            }

            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            var pathToSaveAssembly = this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);

            if (this.fileSystemAccessor.IsFileExists(pathToSaveAssembly))
            {
                throw new QuestionnaireAssemblyAlreadyExistsException(
                    "Questionnaire assembly file already exists and can not be overwritten",
                    new QuestionnaireIdentity(questionnaireId, questionnaireVersion));
            }

            this.fileSystemAccessor.WriteAllBytes(pathToSaveAssembly, assembly);

            this.fileSystemAccessor.MarkFileAsReadonly(pathToSaveAssembly);
        }

        public Task StoreAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly)
        {
            throw new NotImplementedException();
        }

        public void RemoveAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            var pathToSaveAssembly = this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);

            Logger.Info($"Trying to delete assembly for questionnaire {new QuestionnaireIdentity(questionnaireId, questionnaireVersion)}");

            //loaded assembly could be locked
            try
            {
                this.fileSystemAccessor.DeleteFile(pathToSaveAssembly);
            }
            catch (IOException e)
            {
                Logger.Error($"Error on assembly deletion for questionnaire {new QuestionnaireIdentity(questionnaireId, questionnaireVersion)}");
                Logger.Error(e.Message, e);
            }
        }

        public void RemoveAssembly(QuestionnaireIdentity questionnaireIdentity)
        {
            throw new NotImplementedException();
        }

        public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            byte[] assemblyAsByteArray = this.GetAssemblyAsByteArray(questionnaireId, questionnaireVersion);

            if (assemblyAsByteArray == null)
                return null;

            return Convert.ToBase64String(assemblyAsByteArray);
        }

        public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            string pathToAssembly = this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);

            if (!this.fileSystemAccessor.IsFileExists(pathToAssembly))
                return null;

            return this.fileSystemAccessor.ReadAllBytes(pathToAssembly);
        }

        public bool IsQuestionnaireAssemblyExists(Guid questionnaireId, long questionnaireVersion)
        {
            return this.fileSystemAccessor.IsFileExists(this.GetFullPathToAssembly(questionnaireId, questionnaireVersion));
        }

        private string GetAssemblyFileName(Guid questionnaireId, long questionnaireVersion)
        {
            return String.Format("assembly_{0}_v{1}.dll", questionnaireId, questionnaireVersion);
        }
    }
}