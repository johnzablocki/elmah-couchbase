using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase;
using System.Collections;
using Couchbase.Configuration;
using Elmah;
using Enyim.Caching.Memcached;
using Newtonsoft.Json;

namespace Elmah.Couchbase
{
	/// <summary>
	/// ErrorLog implementation using Couchbase Server 2.0 as backing store.
	/// </summary>
	public class CouchbaseErrorLog : ErrorLog
	{
		private static CouchbaseClient _client;
		
		/// <summary>
		/// Initialize new instance of CouchbaseClient using either default config
		/// or specified configuration name
		/// </summary>		
		public CouchbaseErrorLog(IDictionary config)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			if (_client == null)
			{
				if (config.Contains("couchbaseConfigSection"))
				{
					_client = CouchbaseClientFactory.CreateCouchbaseClient(config["couchbaseConfigSection"] as string);
				}
				else
				{
					_client = CouchbaseClientFactory.CreateCouchbaseClient();
				}

				if (config.Contains("applicationName"))
				{
					ApplicationName = config["applicationName"] as string;
				}	
			}			
		}

		/// <summary>
		/// Get error log entry by id
		/// </summary>
		public override ErrorLogEntry GetError(string id)
		{
			if (string.IsNullOrEmpty(id))
				throw new ArgumentNullException("id");

			Guid errorGuid;
			try
			{
				errorGuid = new Guid(id);
			}
			catch (FormatException e)
			{
				throw new ArgumentException(e.Message, "id", e);
			}

			var errorJson = _client.Get<string>(id);
			var error = JsonConvert.DeserializeObject<Error>(errorJson);
			return new ErrorLogEntry(this, id, error);
		}

		/// <summary>
		/// Get list of errors for view in Elmah web viewer
		/// </summary>
		public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
		{
			if (pageIndex < 0)
				throw new ArgumentOutOfRangeException("pageIndex", pageIndex, null);

			if (pageSize < 0)
				throw new ArgumentOutOfRangeException("pageSize", pageSize, null);

			var skip = pageSize * pageIndex;

			//this is NOT the most efficient way to page in Couchbase/CouchDB, but is necessary because
			//there is no way to keep state of the startkey between requests
			//see http://www.couchbase.com/docs/couchbase-manual-2.0/couchbase-views-writing-querying-pagination.html
			//for more information
			var view = _client.GetView("errors", "by_date").Descending(true).Skip(skip).Limit(pageSize);
			
			foreach (var item in view)
			{
				var errorLogEntry = GetError(item.ItemId);
				errorEntryList.Add(errorLogEntry);	
			}				
			
			return view.TotalRows;
		}

		/// <summary>
		/// Log an error
		/// </summary>
		public override string Log(Error error)
		{
			if (error == null)
				throw new ArgumentNullException("error");

			var key = Guid.NewGuid().ToString();
			_client.Store(StoreMode.Set, key, JsonConvert.SerializeObject(error));

			return key;
		}

		/// <summary>
		/// Name displayed in ELMAH viewer
		/// </summary>
		public override string Name
		{
			get
			{
				return "Couchbase Server Error Log";
			}
		}		
		
	}
}

#region [ License information          ]
/* ************************************************************
 * 
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2012 Couchbase, Inc.
 *    
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *    
 *        http://www.apache.org/licenses/LICENSE-2.0
 *    
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *    
 * ************************************************************/
#endregion
