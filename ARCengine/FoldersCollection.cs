﻿using System.Data.SqlClient;
using ARCengine.Collections;
using ARCengine.Interfaces;
using ARCengine.CoreObjects;

namespace ARCengine.Collections
{
    public class FoldersCollection : CoreCollection
    {
        #region constructors
        public FoldersCollection(ICollectionOwner owner) : base(typeof(Folder))
        {
            mOwner = owner;
        }
        #endregion

        #region members
        private ICollectionOwner mOwner;
        #endregion

        #region properties
        public Folder this[int index]
        {
            get
            {
                Refresh(true);
                return (Folder)List[index];
            }
            set
            {
                List[index] = value;
            }
        }
        public ICollectionOwner Owner
        {
            get
            {
                return mOwner;
            }
        }
        public Database Database
        {
            get
            {
                if (mOwner is Database)
                {
                    return (Database)mOwner;
                }
                if(mOwner is Folder)
                {
                    return ((Folder)mOwner).Database;
                }

                return null;
            }
        }
        #endregion

        #region methods
        /// <summary>
        /// adds folder to the collection
        /// </summary>
        /// <param name="folder">new folder to add</param>
        public override string ToString()
        {
            return ToString(1);
        }

        /// <summary>
        /// Converts Folder to a string including SubFolders till designated level.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public string ToString(int level)
        {
            string result = string.Empty;
            foreach (Folder folder in List)
            {
                result += new string('-', level) + folder.Name + '\n';
                result += folder.SubFolders.ToString(level + 1);
            }
            return result;
        }

        /// <summary>
        /// Adds a Folder to teh collection, assigning it's parent to the owner of the collection.
        /// </summary>
        /// <param name="folder"></param>
        public void Add(Folder folder)
        {
            List.Add(folder);
            folder.Parent = mOwner;
        }

        /// <summary>
        /// populates folders by calling Database.prcFolders_children() for roots.
        /// </summary>
        protected override void Read()
        {
            Clear();
            if ((mOwner == null) || (mOwner is Database))
            {
                using (SqlDataReader dr = Database.ExecuteDataReader("prcFolders_tree"))
                {
                    Read(dr);
                    dr.Close();
                }
            }
        }

        /// <summary>
        /// populates folders reculsivelly.
        /// </summary>
        /// <param name="dr"></param>
        private void Read(SqlDataReader dr)
        {
            while (dr.Read())
            {
                // create a folder for current dr record:
                Folder f = new Folder(Database, dr);

                // if owner is a Database, or parentID is 0, then it's a root:
                if ((mOwner is Database) && (f.ParentID == 0))
                {
                    Add(f);
                }

                // if current folder is the new folder's parent, add it to it:
                else if (((Folder)Owner).ID == f.ParentID)
                {
                    Add(f);
                }
                else
                {
                    // search for the parent:
                    ICollectionOwner parent = ((Folder)Owner).FindParent(f.ParentID);
                    if (parent is Database)
                    {
                        ((Database)parent).Folders.Add(f);
                    }
                    else if(parent is Folder)
                    {
                        ((Folder)parent).SubFolders.Add(f);
                    }
                }
                // call reculsivelly:
                f.SubFolders.Read(dr);
            }
        }

        /// <summary>
        /// Saves Collection to the Database.
        /// </summary>
        public void Save()
        {
            foreach (Folder subfolder in List)
            {
                if (subfolder.IsDirty)
                {
                    subfolder.Save();
                }
            }
        }
         #endregion
    }
}