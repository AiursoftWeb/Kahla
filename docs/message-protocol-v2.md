```json
{
    "preview": "Preview context",
    "segments": [
        {
            "type": "text",
            "content": "This is a text segment."
        },
        {
            "type": "image",
            "content": {
                "file": "/path/to/probe/file",
                "alt": "Image alt text (optional)",
                "width": 100,
                "height": 100
            }
        },
        {
            "type": "voice",
            "content": {
                "file": "/path/to/probe/file",
                "alt": "Image alt text (optional)"
            }
        },
        {
            "type": "video",
            "content": {
                "file": "/path/to/probe/file",
                "alt": "Image alt text (optional)"
            }
        },
        {
            "type": "file",
            "content": {
                "file": "/path/to/probe/file",
                "alt": "Image alt text (optional)"
            }
        },
        {
            "type": "contact-card",
            "content": {
                "userid": "[UUID]",
                "name": "Name",
                "avatar": "/path/to/avatar"
            }
        },
        {
            "type": "thread-invitation",
            "content": {
                "token": "Thread invitation token",
                "id": "thread id",
                "name": "Thread name",
                "avatar": "/path/to/avatar"
            }
        }
    ]
}
```
