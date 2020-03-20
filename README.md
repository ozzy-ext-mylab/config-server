# MyLab.ConfigServer

[![Docker image](https://img.shields.io/docker/v/ozzyext/mylab-config-server?sort=semver)](https://hub.docker.com/r/ozzyext/mylab-config-server)

Веб-приложение, предоставляющее доступ к конфигурационным данным по протоколу `HTTP`.

## Использование

Предоставляемая конфигурация доступна через `REST-API` по относительному адресу `/api/config`. Выбор конфигурации осуществляется по `user-id` при `Basic`-авторизации. 

Пример запроса конфигурации:

```
GET /api/config HTTP/1.1

Host: localhost:54004
Authorization: Basic Zm9vOnJpZ2h0LXBhc3M=
User-Agent: insomnia/7.1.1
Accept: */*
```

Пример ответа:

```
HTTP/1.1 200 OK

Transfer-Encoding: chunked
Content-Type: text/plain; charset=utf-8
Server: Microsoft-IIS/10.0
X-Powered-By: ASP.NET
Date: Thu, 19 Mar 2020 15:13:50 GMT

{
  "FooParam": "FooVal",
  "ParamForOverride": "NewVal",
  "SecretParam": "this is a secret",
  "InnerObject": {
    "Bar": "NewBarVal"
  },
  "Redis": {
    "Host": "redis.host.ru",
    "DbIndex": "3",
    "Password": "redis-pass-secret"
  },
  "Rabbit": {
    "Host": "rabbit.host.ru",
    "Login": "login",
    "Password": "rabbit-pass-secret"
  }
}
```

## Сборка конфигураций
![](./doc/my-lab-config-server.png)


На схеме выше описан процесс формирования конфигурации на тестовом примере:

1. В директории `Configs` выбирается базовая конфигурация по `UserId` из параметров `Basic`-авторизации;
2. В директории `Overrides` выбирается переопределяющая конфигурация по `UserId` из параметров `Basic`-авторизации;
3. Осуществляется объединение конфигураций. Отмечены переопределённые параметры;
4. Из директории `Incudes` выбираются включаемые конфигурации, указанные в объединённом на этапе 3 конфиге. Также выбираются включаемые конфигурации, которые указаны в выбранных включаемых конфигурациях - поддерживается вложенность включения конфигураций;
5. Осуществляется поэтапная сборка конфигураций включения в соответствии с вложенностью;
6. В результате получается консолидированная включаемая конфигурация;
7. Объединённая конфигурация объединяется с включаемыми конфигурациями;
8. В результате получается результирующая конфигурация без секретов;
9. Из файла с секретами загружаются секреты, необходимые для заполнения конфигурации
10. Происходит добавление секретов в конфигурацию;
11. В результате получается конфигурация в конечном виде;
12. клиент получает конфигурацию.

## Конфигурация

### Директория Configs

Путь относительно корня приложения: `Resources/Configs`

В этой директории хранятся файлы базовой конфигурации клиентов. Имена файлов соответствуют идентификаторам клиентов с расширениями `json`. Например, файл базовой конфигурации клиента `foo` будет иметь имя `foo.json`.

Директория ориентирована на хранение предопределённых конфигураций из централизованных хранилищ. Например, клонированный репозиторий с конфигурациями.

### Директория Overrides

Путь относительно корня приложения: `Resources/Overrides`

В этой директории хранятся файлы переопределяющей конфигурации клиентов. Имена файлов соответствуют идентификаторам клиентов с расширениями `json`. Например, файл переопределяющей конфигурации клиента `foo` будет иметь имя `foo.json`.

Директория ориентирована на хранение переопределяющих конфигураций, которые заменяют значения в узлах базовой конфигурации. Такие файлы можно использовать для временного и оперативного изменения конфигурации, поэтому их следует создавать и хранить прямо на сервере.

### Директория Include

Путь относительно корня приложения: `Resources/Includes`

В этой директории хранятся файлы включаемой конфигурации клиентов. Имена файлов используются для указания в конфигурациях, куда требуется включение. Для этого в коревом узле конфигурации следует добавить комментарий в формате:

```
//include: include-name
```

, где `include-name` - имя файла в директории `includes` без расширения. Например, в примере выше указано включение конфигурации из файла с именем `include-name.json` из директории `Includes`.

Директория ориентирована на хранение предопределённых конфигураций из централизованных хранилищ. Например, клонированный репозиторий с конфигурациями.

### Файл secrets.json

Путь относительно корня приложения: `Resources/secrets.json`

Файл с секретами. Пример содержания файла:

```json
[
  { "key": "some-secret", "value": "some-val" },
  { "key": "redis-pass", "value": "redis-pass-secret" },
  { "key": "rabbit-pass", "value": "rabbit-pass-secret" }
]
```

Из этого файла берутся секретные параметры для вставке в конфигурационные данные перед отправкой клиенту. Например, в таком файле можно ранить пароли подключения к БД. 

Для указания секрета в конфигурации в качестве значения следует указать имя секрета в следующем виде:

```json
{
  "secret-property": "[secret:redis-pass]"
}
```

, где

* secret-property - наименование поля с секретным значением
* redis-secret - ключ секрета, по которому секрет можно найти в файле `secrets.json`.

Файл `secrets.json` следует хранить на сервере и тщательно ограничить к нему доступ.

### Файл clients.json

Путь относительно корня приложения: `Resources/clients.json`

Файл, содержащий реквизиты `Basic`-авторизации клиентов. Пример содержания файла:

```json
[
  { "login": "foo", "secret": "right-pass" }
]
```

Секрет содержится в открытом виде. Этот файл следует хранить на сервере и тщательно ограничить к нему доступ.

Выбор файлов базовой и переопределяющей конфигураций осуществляется с использованием логина авторизирующегося клиента.

## Пользовательский интерфейс

Конфигурационный сервер имеет веб-интерфейс.

![](./doc/main-web-page.png)

Каждый раздел веб-интерфейса состоит из следующих частей:

1. Заголовок приложения;
2. Раздел конфигураций (конечных);
3. Раздел базовых конфигураций;
4. Раздел переопределяющих конфигураций;
5. Раздел включаемых конфигураций;
6. Раздел с клиентами;
7. Содержательная часть раздела.

В каждом разделе можно посмотреть соответствующие конфигурации и данные, о которых говорилось в разделах выше. 

Ниже приведён пример отображения конфигурации для клиента `foo`.  Отображается:

1. Содержание конфигурации;
2. Информация о секретах в этой конфигурации;
3. Отметка о том, определён ли найденный секрет в файле `secrets.json`;
4. Путь секрета в приведённой конфигурации.

![web-config-example](./doc/web-config-example.png)

