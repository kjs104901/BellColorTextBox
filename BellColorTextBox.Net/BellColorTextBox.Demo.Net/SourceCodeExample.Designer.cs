﻿//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BellColorTextBox.Demo.Net {
    using System;
    
    
    /// <summary>
    ///   지역화된 문자열 등을 찾기 위한 강력한 형식의 리소스 클래스입니다.
    /// </summary>
    // 이 클래스는 ResGen 또는 Visual Studio와 같은 도구를 통해 StronglyTypedResourceBuilder
    // 클래스에서 자동으로 생성되었습니다.
    // 멤버를 추가하거나 제거하려면 .ResX 파일을 편집한 다음 /str 옵션을 사용하여 ResGen을
    // 다시 실행하거나 VS 프로젝트를 다시 빌드하십시오.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SourceCodeExample {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SourceCodeExample() {
        }
        
        /// <summary>
        ///   이 클래스에서 사용하는 캐시된 ResourceManager 인스턴스를 반환합니다.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BellColorTextBox.Demo.Net.SourceCodeExample", typeof(SourceCodeExample).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   이 강력한 형식의 리소스 클래스를 사용하여 모든 리소스 조회에 대해 현재 스레드의 CurrentUICulture 속성을
        ///   재정의합니다.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   using System;
        ///using System.Linq;
        ///using System.Collections.Generic;
        ///
        ///public class QuickSort
        ///{
        ///    public static List&lt;int&gt; Sort(List&lt;int&gt; xs)
        ///    {
        ///        if (!xs.Any())
        ///            return xs;
        ///
        ///        var index = xs.Count() / 2;
        ///        var x = xs[index];
        ///        xs.RemoveAt(index);
        ///        var left = Sort(xs.Where(v =&gt; v &lt;= x).ToList());
        ///        var right = Sort(xs.Where(v =&gt; v &gt; x).ToList());
        ///        return left.Append(x).Concat(right).ToList();
        ///    }
        ///
        ///    public static void ErrorAndExit()
        ///    {
        ///        C[나머지 문자열은 잘림]&quot;;과(와) 유사한 지역화된 문자열을 찾습니다.
        /// </summary>
        internal static string CSharp {
            get {
                return ResourceManager.GetString("CSharp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   {
        ///  &quot;web-app&quot;: {
        ///    &quot;servlet&quot;: [
        ///      {
        ///        &quot;servlet-name&quot;: &quot;cofaxCDS&quot;,
        ///        &quot;servlet-class&quot;: &quot;org.cofax.cds.CDSServlet&quot;,
        ///        &quot;init-param&quot;: {
        ///          &quot;templatePath&quot;: &quot;templates&quot;,
        ///          &quot;templateOverridePath&quot;: &quot;&quot;,
        ///          &quot;defaultListTemplate&quot;: &quot;listTemplate.htm&quot;,
        ///          &quot;defaultFileTemplate&quot;: &quot;articleTemplate.htm&quot;,
        ///          &quot;useJSP&quot;: false,
        ///          &quot;jspListTemplate&quot;: &quot;listTemplate.jsp&quot;,
        ///          &quot;jspFileTemplate&quot;: &quot;articleTemplate.jsp&quot;,
        ///          &quot;cachePackageTagsTr[나머지 문자열은 잘림]&quot;;과(와) 유사한 지역화된 문자열을 찾습니다.
        /// </summary>
        internal static string Json {
            get {
                return ResourceManager.GetString("Json", resourceCulture);
            }
        }
        
        /// <summary>
        ///   syntax = &quot;proto3&quot;;
        ///
        ///option csharp_namespace = &quot;DemoAspNetCore3&quot;;
        ///
        ///service MyOwnService {
        ///  rpc WhoIs(EmptyRequest) returns (WhoIsReply) {}
        ///  rpc IntroduceYourself (IntroduceYourselfRequest) returns (IntroduceYourselfReply) {}
        ///}
        ///
        ///message EmptyRequest {
        ///}
        ///
        ///message WhoIsReply {
        ///  string message = 1;
        ///}
        ///
        ///message IntroduceYourselfRequest {
        ///	string name = 1;
        ///}
        ///
        ///message IntroduceYourselfReply {
        ///	
        ///	string name = 1;
        ///	string job = 2;
        ///	string country = 3;
        ///	repeated string citizenship = 4;
        ///	
        ///	[나머지 문자열은 잘림]&quot;;과(와) 유사한 지역화된 문자열을 찾습니다.
        /// </summary>
        internal static string Proto {
            get {
                return ResourceManager.GetString("Proto", resourceCulture);
            }
        }
        
        /// <summary>
        ///   -- Database-Level
        ///DROP DATABASE databaseName                 -- Delete the database (irrecoverable!)
        ///DROP DATABASE IF EXISTS databaseName       -- Delete if it exists
        ///CREATE DATABASE databaseName               -- Create a new database
        ///CREATE DATABASE IF NOT EXISTS databaseName -- Create only if it does not exists
        ///SHOW DATABASES                             -- Show all the databases in this server
        ///USE databaseName                           -- Set the default (current) database
        ///SELECT DATABASE()        [나머지 문자열은 잘림]&quot;;과(와) 유사한 지역화된 문자열을 찾습니다.
        /// </summary>
        internal static string Sql {
            get {
                return ResourceManager.GetString("Sql", resourceCulture);
            }
        }
    }
}
