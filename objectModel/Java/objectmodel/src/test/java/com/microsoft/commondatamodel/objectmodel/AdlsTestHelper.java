// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

package com.microsoft.commondatamodel.objectmodel;

import com.microsoft.commondatamodel.objectmodel.storage.AdlsAdapter;
import com.microsoft.commondatamodel.objectmodel.utilities.StringUtils;
import org.testng.SkipException;

import static org.testng.Assert.assertFalse;

public class AdlsTestHelper {

    public static void checkADLSEnvironment()
    {
        if (StringUtils.isNullOrEmpty(System.getenv("ADLS_RUNTESTS")))
        {
            // this will cause tests to appear as "Skipped" in the final result
            throw new SkipException("ADLS environment not set up");
        }
    }

    public static AdlsAdapter createAdapterWithSharedKey() {
        return createAdapterWithSharedKey("");
    }

    public static AdlsAdapter createAdapterWithSharedKey(String rootRelativePath) {
        String hostname = System.getenv("ADLS_HOSTNAME");
        String rootPath = System.getenv("ADLS_ROOTPATH");
        String sharedKey = System.getenv("ADLS_SHAREDKEY");

        assertFalse(StringUtils.isNullOrEmpty(hostname));
        assertFalse(StringUtils.isNullOrEmpty(rootPath));
        assertFalse(StringUtils.isNullOrEmpty(sharedKey));

        return new AdlsAdapter(hostname, getFullRootPath(rootPath, rootRelativePath), sharedKey);
    }

    public static AdlsAdapter createAdapterWithClientId() {
        return createAdapterWithClientId("");
    }

    public static AdlsAdapter createAdapterWithClientId(String rootRelativePath) {
        String hostname = System.getenv("ADLS_HOSTNAME");
        String rootPath = System.getenv("ADLS_ROOTPATH");
        String tenant = System.getenv("ADLS_TENANT");
        String clientId = System.getenv("ADLS_CLIENTID");
        String clientSecret = System.getenv("ADLS_CLIENTSECRET");

        assertFalse(StringUtils.isNullOrEmpty(hostname));
        assertFalse(StringUtils.isNullOrEmpty(rootPath));
        assertFalse(StringUtils.isNullOrEmpty(tenant));
        assertFalse(StringUtils.isNullOrEmpty(clientId));
        assertFalse(StringUtils.isNullOrEmpty(clientSecret));

        return new AdlsAdapter(hostname, getFullRootPath(rootPath, rootRelativePath), tenant, clientId, clientSecret);
    }

    public static String getFullRootPath(String rootPath, String rootRelativePath)
    {
        if (rootRelativePath == null || rootRelativePath.isEmpty()) {
            return rootPath;
        }

        if (rootPath.endsWith("/")) {
            rootPath = rootPath.substring(0, rootPath.length() - 1);
        }

        if (rootRelativePath.startsWith("/")) {
            rootRelativePath = rootRelativePath.substring(1);
        }

        return rootPath + "/" + rootRelativePath;
    }
}
