﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using Mine.Models;
using Mine.Views;

namespace Mine.ViewModels
{
    public class ItemIndexVeiwModel : BaseViewModel
    {
        public ObservableCollection<ItemModel> DataSet { get; set; }
        public Command LoadItemsCommand { get; set; }

        public ItemIndexVeiwModel()
        {
            Title = "Items";
            DataSet = new ObservableCollection<ItemModel>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            MessagingCenter.Subscribe<ItemCreatePage, ItemModel>(this, "AddItem", async (obj, item) =>
            {
                var newItem = item as ItemModel;
                DataSet.Add(newItem);
                await DataStore.CreateAsync(newItem);
            });

            MessagingCenter.Subscribe<ItemDeletePage, ItemModel>(this, "DeleteItem", async (obj, item) =>
            {
                var data = item as ItemModel;
                await DeleteAsync(data);
            });

            MessagingCenter.Subscribe<ItemUpdatePage, ItemModel>(this, "UpdateItem", async (obj, item) =>
            {
                var data = item as ItemModel;
                await UpdateAsync(data);
            });
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                DataSet.Clear();
                var items = await DataStore.IndexAsync(true);
                foreach (var item in items)
                {
                    DataSet.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
        /// <summary>
        /// Read an item from the datatstore
        /// </summary>
        /// <param name="id">ID of the record</param>
        /// <returns>The Record from ReadAsync</returns>
        public async Task<ItemModel> ReadAsync(string id)
        {
            var result = await DataStore.ReadAsync(id);
            return result;
        }
        /// <summary>
        /// Delete an item from the database
        /// </summary>
        /// <param name="id">The Record to delete</param>
        /// <returns>True if Deleted</returns>
        public async Task<bool> DeleteAsync(ItemModel data)
        {
            //Check if record exists, if it does not, then null is returned
            var record = await ReadAsync(data.Id);
            if(record == null)
            {
                return false;
            }
            //Remove the local dataset.cache
            DataSet.Remove(data);

            //Call to remove it from the DataStore
            var result = await DataStore.DeleteAsync(data.Id);

            return result;
        }

        /// <summary>
        /// Delete an item from the database
        /// </summary>
        /// <param name="id">The Record to delete</param>
        /// <returns>True if Deleted</returns>
        public async Task<bool> UpdateAsync(ItemModel data)
        {
            //Check if record exists, if it does not, then null is returned
            var record = await ReadAsync(data.Id);
            if (record == null)
            {
                return false;
            }            
            //Call to update it in the DataStore
            var result = await DataStore.UpdateAsync(data);

            var canExecute = LoadItemsCommand.CanExecute(null);
            LoadItemsCommand.Execute(null);

            return result;
        }
    }
}