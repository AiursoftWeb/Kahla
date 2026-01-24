# Kahla Message Protocol V2

本文档描述了 Kahla 消息体的 DSL（领域特定语言）结构。

## 消息根结构 (MessageContent)

```json
{
    "v": 2,
    "segments": [...]
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `v` | `number` | 协议版本号 |
| `segments` | `MessageSegmentBase[]` | 消息段数组 |

## 消息段类型 (MessageSegmentTypes)

支持以下消息段类型：

- `text` - 文本消息
- `image` - 图片消息
- `video` - 视频消息
- `voice` - 语音消息
- `file` - 文件消息
- `contact` - 联系人名片
- `thread-invitation` - 群组邀请
- `thread-join-request` - 加群请求

---

## 各消息段详细结构

### 1. 文本消息 (MessageSegmentText)

```json
{
    "type": "text",
    "content": "这是一条纯文本消息"
}
```

或带有注解的文本：

```json
{
    "type": "text",
    "content": [
        "你好，",
        {
            "annotated": "mention",
            "content": "@张三",
            "targetId": "uuid-of-user"
        },
        " 欢迎加入！"
    ]
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `type` | `'text'` | 固定值 |
| `content` | `string \| MessageTextWithAnnotate[]` | 纯文本字符串，或混合了字符串与注解对象的数组 |

#### 文本注解 (MessageTextAnnotated)

目前支持的注解类型：

##### @提及 (MessageTextAnnotatedMention)

```json
{
    "annotated": "mention",
    "content": "@用户名",
    "targetId": "user-uuid"
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `annotated` | `'mention'` | 注解类型 |
| `content` | `string` | 显示的文本内容 |
| `targetId` | `string` | 被提及用户的 UUID |

---

### 2. 图片消息 (MessageSegmentImage)

```json
{
    "type": "image",
    "url": "/path/to/image",
    "width": 1920,
    "height": 1080,
    "alt": "图片描述（可选）"
}
```

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `type` | `'image'` | 是 | 固定值 |
| `url` | `string` | 是 | 图片资源路径 |
| `width` | `number` | 是 | 图片宽度（像素） |
| `height` | `number` | 是 | 图片高度（像素） |
| `alt` | `string` | 否 | 图片替代文本 |

---

### 3. 视频消息 (MessageSegmentVideo)

```json
{
    "type": "video",
    "url": "/path/to/video"
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `type` | `'video'` | 固定值 |
| `url` | `string` | 视频资源路径 |

---

### 4. 语音消息 (MessageSegmentVoice)

```json
{
    "type": "voice",
    "url": "/path/to/audio",
    "duration": 15
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `type` | `'voice'` | 固定值 |
| `url` | `string` | 音频资源路径 |
| `duration` | `number` | 语音时长（秒） |

---

### 5. 文件消息 (MessageSegmentFile)

```json
{
    "type": "file",
    "url": "/path/to/file",
    "fileName": "document.pdf",
    "size": 1048576
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `type` | `'file'` | 固定值 |
| `url` | `string` | 文件资源路径 |
| `fileName` | `string` | 文件名 |
| `size` | `number` | 文件大小（字节） |

---

### 6. 联系人名片 (MessageSegmentContact)

```json
{
    "type": "contact",
    "id": "user-uuid"
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `type` | `'contact'` | 固定值 |
| `id` | `string` | 用户 UUID |

---

### 7. 群组邀请 (MessageSegmentThreadInvitation)

```json
{
    "type": "thread-invitation",
    "id": 12345,
    "targetUserId": "target-user-uuid",
    "token": "invitation-token",
    "validTo": 1735689600000
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `type` | `'thread-invitation'` | 固定值 |
| `id` | `number` | 群组 ID |
| `targetUserId` | `string` | 目标用户的 UUID |
| `token` | `string` | 邀请令牌 |
| `validTo` | `number` | 有效期（时间戳，毫秒） |

---

### 8. 加群请求 (MessageSegmentThreadJoinRequest)

```json
{
    "type": "thread-join-request",
    "id": "request-id",
    "token": "request-token",
    "validTo": "2025-12-31T23:59:59.000Z"
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `type` | `'thread-join-request'` | 固定值 |
| `id` | `string` | 请求 ID |
| `token` | `string` | 请求令牌 |
| `validTo` | `Date` | 有效期（ISO 日期格式） |

---

## 完整示例

```json
{
    "v": 2,
    "segments": [
        {
            "type": "text",
            "content": [
                "大家好，",
                {
                    "annotated": "mention",
                    "content": "@所有人",
                    "targetId": "all"
                },
                "！请查看下面的文件："
            ]
        },
        {
            "type": "image",
            "url": "/files/screenshot.png",
            "width": 800,
            "height": 600,
            "alt": "项目截图"
        },
        {
            "type": "file",
            "url": "/files/report.pdf",
            "fileName": "年度报告.pdf",
            "size": 2097152
        }
    ]
}
```
