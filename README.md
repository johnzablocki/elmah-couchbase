elmah-couchbase
===========================

Logging provider for using Couchbase Server 2.0 with ELMAH.  

#Usage

##Configure the Couchbase .NET Client Library

    <section name="couchbase" type="Couchbase.Configuration.CouchbaseClientSection, Couchbase" />

    <couchbase>
	  <documentNameTransformer type="Couchbase.Configuration.DevelopmentModeNameTransformer, Couchbase" />
	  <httpClientFactory type="Couchbase.HammockHttpClientFactory, Couchbase" />
      <servers bucket="default">
        <add uri="http://127.0.0.1:8091/pools" />
      </servers>    
    </couchbase> 

##Configure elmah-couchbase

    <elmah>
		<errorLog type="Elmah.Couchbase.CouchbaseErrorLog, Elmah.Couchbase" couchbaseConfigSection="<other-than-default-couchbase-section-name>" />
    </elmah> 

To learn how to configure multiple buckets, see http://www.couchbase.com/wiki/display/couchbase/Couchbase+.NET+Client+Library.

#Notes

Couchbase Server 2.0 required.  Developer preview version of .NET Client Library for Couchbase included in "lib" directory.

Be sure to deploy the view found in CouchbaseErrorLog.json at the root of the provider.