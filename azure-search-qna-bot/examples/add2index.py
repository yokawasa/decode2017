#!/usr/bin/env python
# -*- coding: utf-8 -*-

import sys

import json
import httplib, urllib, base64
import time

azuresearch_api_url = '<your-service-name>.search.windows.net'
azuresearch_api_key = '<your-azure-search-api-key>'
azuresearch_api_version = '2016-09-01-Preview'
keypthrase_api_subkey='<your-key-phrase-api-subscription-key>'

document_language = 'ja'

def get_keyphrases( doc_lang, doc_id, doc_text):
    headers = {
        # Request headers
        'Content-Type': 'application/json',
        'Ocp-Apim-Subscription-Key': keypthrase_api_subkey,
    }

    params = urllib.urlencode({
    })

    REQ_BODY= json.dumps(
            {
                "documents": [
                    {
                        "language": doc_lang,
                        "id": doc_id,
                        "text": doc_text
                    }
                ]
            }
        )

    data = ''
    try:
        conn = httplib.HTTPSConnection('westus.api.cognitive.microsoft.com')
        conn.request("POST", "/text/analytics/v2.0/keyPhrases?%s" % params, REQ_BODY, headers)
        response = conn.getresponse()
        data = response.read()
        print(data)
        conn.close()
    except Exception as e:
        print("[Errno {0}] {1}".format(e.errno, e.strerror))

    res=json.loads(data)
    if isinstance(res, dict) and 'errors' in res and len(res['errors']) > 0:
        for error in res['errors']:
            print("[Error id:{0}] {1}".format(error['id'], error['message']))
        return []

    phrase_list = []
    for doc in res['documents']:
        for p in doc['keyPhrases']:
            if p:
                phrase_list.append(p)
    return phrase_list


class AzureSearchClient:
    def __init__(self, azuresearch_api_url, azuresearch_api_key, azuresearch_api_version):
        self.azuresearch_api_url=azuresearch_api_url
        self.azuresearch_api_key=azuresearch_api_key
        self.azuresearch_api_version=azuresearch_api_version
        self.headers={
            'Content-Type': "application/json; charset=UTF-8",
            'Api-Key': self.azuresearch_api_key,
            'Accept': "application/json", 'Accept-Charset':"UTF-8"
        }

    def add_documents(self,index_name, documents, merge):
        #raise ConfigError, 'no index_name' if index_name.empty?
        #raise ConfigError, 'no documents' if documents.empty?
        #action = merge ? 'mergeOrUpload' : 'upload'
        action = 'mergeOrUpload' if merge else 'upload'
        for document in documents:
            document['@search.action'] = action
        
        # Create JSON string for request body
        import simplejson as json
        reqobjects={}
        reqobjects['value'] = documents
        from StringIO import StringIO
        io=StringIO()
        json.dump(reqobjects, io)
        req_body = io.getvalue()
        # HTTP request to Azure search REST API
        import httplib
        conn = httplib.HTTPSConnection(self.azuresearch_api_url)
        conn.request("POST",
                "/indexes/{0}/docs/index?api-version={1}".format(index_name, self.azuresearch_api_version),
                req_body, self.headers)
        response = conn.getresponse()
        print "status:", response.status, response.reason
        data = response.read()
        print "data:", data
        conn.close()

if __name__ == '__main__':
    argvs = sys.argv
    argc = len(argvs)
    if (argc != 4):
        print 'Usage: # python %s <tsvfile> <indexname> <category>' % argvs[0]
        quit()
   
    tsvfile = argvs[1];
    indexname = argvs[2];
    category = argvs[3];
    documents = []
    docnum = 0
    import csv
    with open(tsvfile, 'rb') as f:
        reader = csv.reader(f,delimiter='\t')
        next(reader)   # skip header
        #Question    Answer  Source
        for row in reader:
            question=row[0].decode('utf8')
            answer=row[1].decode('utf8')
            url=row[2]
            docid="{}{}".format(category,docnum)
            tags =  get_keyphrases(document_language, docid, question)
            document = {
                "id" : docid,
                "question": question,
                "answer": answer,
                "category": category,
                "url": url,
                "tags": tags
            }
            print document
            documents.append(document)
            docnum = docnum + 1

    client=AzureSearchClient( azuresearch_api_url, azuresearch_api_key, azuresearch_api_version)
    client.add_documents(indexname, documents, 'upload')

