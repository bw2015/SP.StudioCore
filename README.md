# SP.StudioCore

## Data 数据库操作
> 支持SQLServer/MySQL

### 数据库操作示例
```
  public class DemoAgent : DbAgent<DemoAgent>{

    // 构造函数中进行数据库链接的初始化
    public DemoAgent():base("ConnectionString"){}
  
    public void Execute(string sql){
      using(DbExecutor db = NewExecutor()){
        db.Execute(sql);
      }
    }
  }
```
