﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Swsu.Tools.DbBackupper.Resources {
    using System;
    
    
    /// <summary>
    ///   Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    /// </summary>
    // Этот класс создан автоматически классом StronglyTypedResourceBuilder
    // с помощью такого средства, как ResGen или Visual Studio.
    // Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    // с параметром /str или перестройте свой проект VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages() {
        }
        
        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Swsu.Tools.DbBackupper.Resources.Messages", typeof(Messages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Выбранная БД имеет активные соединения. Вы хотите отключить их?.
        /// </summary>
        public static string ActiveConnectionsAbortingRequest {
            get {
                return ResourceManager.GetString("ActiveConnectionsAbortingRequest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Выполняется продолжительная операция. Вы хотите прервать процесс?.
        /// </summary>
        public static string AppInterruptionConfirmation {
            get {
                return ResourceManager.GetString("AppInterruptionConfirmation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Приложение успешно запущено.
        /// </summary>
        public static string AppStartSuccess {
            get {
                return ResourceManager.GetString("AppStartSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Ошибка создания резервной копии.
        /// </summary>
        public static string BackupFailed {
            get {
                return ResourceManager.GetString("BackupFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Резервирование.
        /// </summary>
        public static string Backuping {
            get {
                return ResourceManager.GetString("Backuping", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на При резервировании произошли ошибки!.
        /// </summary>
        public static string BackupProcessFailed {
            get {
                return ResourceManager.GetString("BackupProcessFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Резервирование завершилось успешно!.
        /// </summary>
        public static string BackupProcessSucceed {
            get {
                return ResourceManager.GetString("BackupProcessSucceed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Резервная копия успешно создана.
        /// </summary>
        public static string BackupSucceed {
            get {
                return ResourceManager.GetString("BackupSucceed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Бинарные файлы резервных копий.
        /// </summary>
        public static string BinaryTypeFiles {
            get {
                return ResourceManager.GetString("BinaryTypeFiles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Невозможно завершить приложение из-за запущенной длительной операции.
        /// </summary>
        public static string CantStopApplication {
            get {
                return ResourceManager.GetString("CantStopApplication", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Ошибка получения копии NpgsqlConnectionStringBuilder.
        /// </summary>
        public static string ConnectionBuilderGettingError {
            get {
                return ResourceManager.GetString("ConnectionBuilderGettingError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Проверка соединения.
        /// </summary>
        public static string ConnectionCheck {
            get {
                return ResourceManager.GetString("ConnectionCheck", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Невозможно подключиться к серверу БД.
        /// </summary>
        public static string ConnectionDenied {
            get {
                return ResourceManager.GetString("ConnectionDenied", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Ошибка создания строки соединения.
        /// </summary>
        public static string ConnectionStringBuildingError {
            get {
                return ResourceManager.GetString("ConnectionStringBuildingError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Ошибка создания БД.
        /// </summary>
        public static string CreateDbFailed {
            get {
                return ResourceManager.GetString("CreateDbFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на База данных успешно создана.
        /// </summary>
        public static string CreateDbSucceed {
            get {
                return ResourceManager.GetString("CreateDbSucceed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на База данных с таким именем уже существует. Вы хотите заменить ее?.
        /// </summary>
        public static string DbAlreadyExistsWarning {
            get {
                return ResourceManager.GetString("DbAlreadyExistsWarning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Не задано имя БД.
        /// </summary>
        public static string DbNameIsNull {
            get {
                return ResourceManager.GetString("DbNameIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Получение структуры БД.
        /// </summary>
        public static string DbStructureGetting {
            get {
                return ResourceManager.GetString("DbStructureGetting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Завершился процесс создания резервной копии.
        /// </summary>
        public static string DumpProcessFinished {
            get {
                return ResourceManager.GetString("DumpProcessFinished", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Начался процесс создания резервной копии.
        /// </summary>
        public static string DumpProcessStarted {
            get {
                return ResourceManager.GetString("DumpProcessStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Список объектов БД пуст.
        /// </summary>
        public static string EmptyDb {
            get {
                return ResourceManager.GetString("EmptyDb", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Ошибка получения структуры БД.
        /// </summary>
        public static string GetDbStructureError {
            get {
                return ResourceManager.GetString("GetDbStructureError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Резервная копия не сделана т.к. не выбран ни один объект резервирования.
        /// </summary>
        public static string InvalidObjectsCountForBackuping {
            get {
                return ResourceManager.GetString("InvalidObjectsCountForBackuping", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Неверный формат IP-адреса сервера.
        /// </summary>
        public static string IpAddressIsInvalid {
            get {
                return ResourceManager.GetString("IpAddressIsInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Создание новой БД.
        /// </summary>
        public static string NewDbCreating {
            get {
                return ResourceManager.GetString("NewDbCreating", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Значение порта некорректно.
        /// </summary>
        public static string PortIsInvalid {
            get {
                return ResourceManager.GetString("PortIsInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Ошибка восстановления из резервной копии.
        /// </summary>
        public static string RestoreFailed {
            get {
                return ResourceManager.GetString("RestoreFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на При восстановлении произошли ошибки!.
        /// </summary>
        public static string RestoreProcessFailed {
            get {
                return ResourceManager.GetString("RestoreProcessFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Завершился процесс восстановления из резервной копии.
        /// </summary>
        public static string RestoreProcessFinished {
            get {
                return ResourceManager.GetString("RestoreProcessFinished", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Начался процесс восстановления из резервной копии.
        /// </summary>
        public static string RestoreProcessStarted {
            get {
                return ResourceManager.GetString("RestoreProcessStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Восстановление завершилось успешно!.
        /// </summary>
        public static string RestoreProcessSucceed {
            get {
                return ResourceManager.GetString("RestoreProcessSucceed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Данные успешно восстановлены из резервной копии.
        /// </summary>
        public static string RestoreSucceed {
            get {
                return ResourceManager.GetString("RestoreSucceed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Восстановление.
        /// </summary>
        public static string Restoring {
            get {
                return ResourceManager.GetString("Restoring", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Код завершения.
        /// </summary>
        public static string ResultCode {
            get {
                return ResourceManager.GetString("ResultCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Ошибка соединения с сервером БД.
        /// </summary>
        public static string ServerConnectionError {
            get {
                return ResourceManager.GetString("ServerConnectionError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Сервер БД доступен.
        /// </summary>
        public static string ServerIsAvailable {
            get {
                return ResourceManager.GetString("ServerIsAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Сервер БД недоступен.
        /// </summary>
        public static string ServerIsNotAvailable {
            get {
                return ResourceManager.GetString("ServerIsNotAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Файлы запросов.
        /// </summary>
        public static string SqlTypeFiles {
            get {
                return ResourceManager.GetString("SqlTypeFiles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Старт приложения.
        /// </summary>
        public static string StartApplication {
            get {
                return ResourceManager.GetString("StartApplication", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Остановка приложения.
        /// </summary>
        public static string StopApplication {
            get {
                return ResourceManager.GetString("StopApplication", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на БД с указанным именем уже существует. Вы хотите перезаписать ее?.
        /// </summary>
        public static string SuchDbAlreadyExists {
            get {
                return ResourceManager.GetString("SuchDbAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Архивные файлы резервных копий.
        /// </summary>
        public static string TarTypeFiles {
            get {
                return ResourceManager.GetString("TarTypeFiles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Не задано имя пользователя.
        /// </summary>
        public static string UserIsNull {
            get {
                return ResourceManager.GetString("UserIsNull", resourceCulture);
            }
        }
    }
}
