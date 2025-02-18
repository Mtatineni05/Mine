﻿using System;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using SQLite;
using Mine.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Mine.Services
{
    public class DatabaseService : IDataStore<ItemModel>
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() => {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        });

        static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;

        public DatabaseService()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(ItemModel).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(ItemModel)).ConfigureAwait(false);
                }
                initialized = true;
            }
        }

        /// <summary>
        /// Inserting an item to database
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<bool> CreateAsync(ItemModel item)
        {
            if(item == null)
            {
                return false;
            }
            var result = await Database.InsertAsync(item);
            if(result == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Update an item to the database
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(ItemModel item)
        {
            if(item == null)
            {
                return false;
            }
            //Database call to update an item
            var result = await Database.UpdateAsync(item);
            if(result == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Delete an item from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string id)
        {
            var data = await ReadAsync(id);
            if(data == null)
            {
                return false;
            }
            //Delete the data where id matches
            var result = await Database.DeleteAsync(data);
            if(result == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read items from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<ItemModel> ReadAsync(string id)
        {
            if(id == null)
            {
                return null;
            }
            //Call database to read ID
            //Using Linq syntax to find the first record that matches with ID
            var result = Database.Table<ItemModel>().FirstOrDefaultAsync(m => m.Id.Equals(id));
            return result;

        }
        /// <summary>
        /// Showing Index List
        /// </summary>
        /// <param name="forceRefresh"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ItemModel>> IndexAsync(bool forceRefresh = false)
        {
            var result = await Database.Table<ItemModel>().ToListAsync();
            return result;
        }
    }
}