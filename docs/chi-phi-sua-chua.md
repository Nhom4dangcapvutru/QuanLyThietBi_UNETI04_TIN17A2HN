# Chi phí sửa chữa

Chi phí dự kiến dùng để báo giá tham khảo. Tiền linh kiện được tính từ các dòng chi tiết. Tổng chi phí thực tế chỉ được chốt khi chuyển phiếu từ Đang xử lý sang Hoàn thành:

`Tổng thực tế = Tiền linh kiện + Tiền công + Chi phí khác - Giảm trừ`

Nhân viên nhập 0 cho khoản không phát sinh. Chi phí khác và giảm trừ lớn hơn 0 phải có nội dung giải thích. Giảm trừ không được vượt tổng trước giảm. Server tính lại tiền linh kiện và tổng; không sử dụng tổng tiền gửi từ trình duyệt. Tiền linh kiện được lưu tại thời điểm nghiệm thu để giữ nguyên bảng phân tích.

Phiếu hoàn thành không được thêm, sửa hoặc xóa linh kiện qua cả hai đường xử lý kỹ thuật và CRUD chi tiết. Linh kiện của phiếu đang xử lý không tự ghi đè tổng thực tế. Dashboard và thống kê tiếp tục cộng cùng trường TongChiPhi của các phiếu Hoàn thành; đây là giá trị nghiệm thu, không xác nhận đã thu tiền.

## Database hiện có

Project hiện khởi tạo Database bằng EnsureCreated. Khi chạy lại phiên bản mới, RepairCostSchema bổ sung 6 cột nullable còn thiếu, không xóa phiếu hoặc cập nhật tổng cũ. Cần quyền ALTER TABLE; nếu SQL Server không sẵn sàng, phải khôi phục kết nối rồi chạy lại chương trình. Không xóa Database để áp dụng thay đổi này.

Phiếu cũ vẫn giữ nguyên tổng đã ghi nhận, kể cả 0 hoặc chưa có tổng. Không tự coi chênh lệch là tiền công. Trang chi tiết hiển thị “Chưa có phân tích chi phí đầy đủ cho phiếu cũ”. Chỉ bộ dữ liệu mẫu được tạo mới có phân tích minh họa rõ ràng.

## Migration

Migration AddRepairCostBreakdown và ModelSnapshot đã được cập nhật. Migration cũng bổ sung các trường quy trình vốn đã có trong Entity nhưng thiếu trong InitialCreate; lệnh thêm cột, index và khóa ngoại có kiểm tra tồn tại để không trùng với Database đã được bổ sung trước đó.

Với Database mới được quản lý hoàn toàn bằng Migration, áp dụng các Migration theo thứ tự. Với Database cũ tạo bằng EnsureCreated và không có __EFMigrationsHistory, dùng cơ chế bổ sung cột ở trên; không chạy InitialCreate lên các bảng đã tồn tại. Việc chuyển toàn bộ Database cũ sang lịch sử Migration là công việc riêng.

## Kiểm tra

- Tổng 250.000 tiền linh kiện + 500.000 tiền công = 750.000.
- Cộng 100.000 chi phí khác và trừ 50.000 ưu đãi = 800.000.
- Giảm trừ toàn bộ có lý do cho phép tổng bằng 0.
- Từ chối số âm, thiếu khoản tiền, giảm trừ vượt tổng, phụ phí hoặc giảm trừ thiếu lý do.
- Giữ nguyên phiếu cũ 750.000 khi chỉ biết 250.000 tiền linh kiện, không tự sinh phần chênh lệch.
- Chưa xác minh lưu nghiệm thu và nâng cấp schema trên SQL Server thực tế trong phiên làm việc này.
