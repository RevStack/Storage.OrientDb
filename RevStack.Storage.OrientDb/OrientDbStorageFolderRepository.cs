using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using RevStack.OrientDb;
using RevStack.Pattern;
using RevStack.Storage.Model;
using RevStack.Storage.Repository;

namespace Storage.OrientDb
{
    public class OrientDbStorageFolderRepository : IStorageFolderRepository
    {
        private readonly OrientDbRepository<StorageFolder, int> _repository;
        private readonly OrientDbRepository<StorageFile, int> _fileRepository;
        public OrientDbStorageFolderRepository(OrientDbRepository<StorageFolder, int> repository, 
            OrientDbRepository<StorageFile, int> fileRepository)
        {
            _repository = repository;
            _fileRepository = fileRepository;
        }

        public IStorageFolder Add(IStorageFolder entity)
        {
            var storageFolder = new StorageFolder();
            storageFolder.Path = entity.Path;
            storageFolder.Files = entity.Files;

            storageFolder = _repository.Add(storageFolder);

            //recurse folders
            int lastIndex = storageFolder.Path.LastIndexOf('/');
            if (lastIndex != -1)
            {
                var path = storageFolder.Path.Substring(0, lastIndex);
                //if (string.IsNullOrEmpty(storageFolder.Path))
                //    storageFolder.Path = "/";
                RecurseForCreateFolderPath(path, storageFolder.Id, storageFolder);
            }

            return storageFolder;
        }

        public void Delete(IStorageFolder entity)
        {
            //RemoveFromCurrentFolder(entity);

            List<IStorageFile> files = entity.Files;

            foreach (IStorageFile file in files)
            {
                _fileRepository.Delete((StorageFile)file);
            }

            List<IStorageFolder> folders = entity.Folders;

            foreach (IStorageFolder folder in folders)
            {
                _repository.Delete((StorageFolder)folder);
            }

            _repository.Delete((StorageFolder)entity);
        }

        public IStorageFolder Get(int id)
        {
            return _repository.Find(c=>c.Id == id).SingleOrDefault();
        }

        public IEnumerable<IStorageFolder> Get()
        {
            return _repository.Get();
        }

        public IStorageFolder Update(IStorageFolder entity)
        {
            var currentFolder = _repository.Find(c => c.Id == entity.Id).SingleOrDefault();

            if (currentFolder != null && currentFolder.Path != entity.Path)
            {
                List<IStorageFile> files = entity.Files;

                foreach (IStorageFile file in files)
                {
                    var filePath = "";
                    var oPath = "";

                    var obj = _fileRepository.Find(c => c.Id == file.Id).SingleOrDefault();
                    filePath = obj.Path;
                    oPath = obj.Path;
                    
                    int index = filePath.LastIndexOf('/');
                    string fileName = filePath;

                    if (index != -1)
                        fileName = filePath.Substring(index + 1);

                    oPath = entity.Path + "/" + fileName;

                    var f = (StorageFile)file;
                    f.Path = oPath;
                    _fileRepository.Update(f);
                }

                List<IStorageFolder> folders = entity.Folders;

                foreach (IStorageFolder folder in folders)
                {
                    var filePath = "";
                    var oPath = "";

                    var obj = Get(folder.Id);
                    filePath = obj.Path;
                    oPath = obj.Path;

                    int index = filePath.LastIndexOf('/');
                    string fileName = filePath;

                    if (index != -1)
                        fileName = filePath.Substring(index + 1);

                    oPath = entity.Path + "/" + fileName;

                    var f = (StorageFolder)folder;
                    f.Path = oPath;
                    _repository.Update(f);
                }

            }

            entity = _repository.Update((StorageFolder)entity);

            return entity;
        }

        #region "private"
        //private void RemoveFromCurrentFolder(IStorageFolder entity)
        //{
        //    //remove from folder 
        //    List<StorageFolder> folders = _repository.Get().Where(c => c.Files.Any(t => t.Id == entity.Id)).ToList();

        //    if (folders.Any())
        //    {
        //        StorageFolder currentFolder = folders[0];
        //        List<IStorageFile> childFiles = currentFolder.Files;
        //        int index = -1;

        //        foreach (IStorageFile file in childFiles)
        //        {
        //            if (file.Id == entity.Id)
        //                index = childFiles.FindIndex(c => c.Id == file.Id);
        //        }

        //        if (index != -1)
        //            childFiles.RemoveAt(index);

        //        currentFolder.Files = childFiles;

        //        //REMOVE FOLDERS


        //        _repository.Update(currentFolder);
        //    }
        //}

        private void RecurseForCreateFolderPath(string path, int childId, IStorageFolder obj)
        {
            //check for root folder
            if (string.IsNullOrEmpty(path))
                path = "/";

            IList<StorageFolder> array = _repository.Find(c => c.Path == path).ToList();

            if (!array.Any())
            {
                //folder does not exist so insert and move up
                StorageFolder folder = new StorageFolder();
                folder.Path = path;
                folder.Folders.Add(obj);
                folder = _repository.Add(folder);

                if (!string.IsNullOrEmpty(path.Trim()))
                {
                    int lastIndex = path.LastIndexOf('/');
                    if (lastIndex != -1)
                    {
                        var id = folder.Id;
                        path = path.Substring(0, lastIndex);
                        RecurseForCreateFolderPath(path, id, folder);
                    }
                }
            }
            //else
            //{
            //    StorageFolder j_obj = array[0];
            //    List<IStorageFile> files = j_obj.Files;
            //    var id = j_obj.Id;

            //    if (id != childId)
            //    {
            //        var list = new List<StorageFile>();
            //        foreach (StorageFile file in files)
            //        {
            //            list.Add(file);
            //        }

            //        if (!list.Where(c => c.Id == childId).Any())
            //            files.Add((StorageFile)obj);
            //    }

            //    j_obj.Files = files;

            //    _repository.Update(j_obj);
            //}
        }


        #endregion
    }
}
