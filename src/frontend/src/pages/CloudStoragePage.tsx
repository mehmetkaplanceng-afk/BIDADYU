import React, { useEffect, useState } from 'react';
import {
  Table,
  Card,
  Tag,
  Button,
  Modal,
  Form,
  Input,
  Upload,
  Select,
  Space,
  Popconfirm,
  Tooltip,
  Typography,
  Badge,
  message,
} from 'antd';
import {
  UploadOutlined,
  HddOutlined,
  FileProtectOutlined,
  DeleteOutlined,
  EditOutlined,
  ReloadOutlined,
  AppstoreOutlined,
  CodeOutlined,
} from '@ant-design/icons';
import api, { getSoftware } from '../services/api';

const { Text } = Typography;

export const CloudStoragePage: React.FC = () => {
  const [packages, setPackages] = useState<any[]>([]);
  const [softwareList, setSoftwareList] = useState<any[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [isUploadModalOpen, setIsUploadModalOpen] = useState<boolean>(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState<boolean>(false);
  const [editingPackage, setEditingPackage] = useState<any>(null);

  const [uploadForm] = Form.useForm();
  const [editForm] = Form.useForm();
  const [fileList, setFileList] = useState<any[]>([]);

  // Form için sürüm seçenekleri
  const [uploadVersions, setUploadVersions] = useState<any[]>([]);
  const [editVersions, setEditVersions] = useState<any[]>([]);

  const fetchPackages = async () => {
    setLoading(true);
    try {
      const res = await api.get('/Packages');
      setPackages(res.data);
    } catch {
      message.error('Paketler yüklenemedi.');
    } finally {
      setLoading(false);
    }
  };

  const fetchSoftware = async () => {
    try {
      const res = await getSoftware();
      setSoftwareList(res.data);
    } catch {
      // Hata yok
    }
  };

  useEffect(() => {
    fetchPackages();
    fetchSoftware();
  }, []);

  // Upload Form: Yazılım seçilince Sürümleri güncelle
  const handleUploadSoftwareChange = (softwareId: string) => {
    const sw = softwareList.find((s) => s.id === softwareId);
    setUploadVersions(sw?.versions || []);
    uploadForm.setFieldsValue({ softwareVersionId: undefined });
  };

  // Edit Form: Yazılım seçilince Sürümleri güncelle
  const handleEditSoftwareChange = (softwareId: string) => {
    const sw = softwareList.find((s) => s.id === softwareId);
    setEditVersions(sw?.versions || []);
    editForm.setFieldsValue({ softwareVersionId: undefined });
  };

  const handleUpload = async (values: any) => {
    if (fileList.length === 0) {
      message.warning('Lütfen bilgisayarınızdan bir paket dosyası seçin!');
      return;
    }

    const formData = new FormData();
    formData.append('file', fileList[0]);
    if (values.softwareVersionId) {
      formData.append('softwareVersionId', values.softwareVersionId);
    }
    formData.append('silentArgs', values.silentArgs || '/qn /norestart');

    try {
      await api.post('/Packages/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      message.success('Paket bulut depoya yüklendi ve ilişkili sürüme atandı!');
      setIsUploadModalOpen(false);
      uploadForm.resetFields();
      setFileList([]);
      setUploadVersions([]);
      fetchPackages();
    } catch (err: any) {
      message.error(err.response?.data?.Message || 'Paket yüklenemedi.');
    }
  };

  const openEditModal = (pkg: any) => {
    setEditingPackage(pkg);
    if (pkg.softwareId) {
      const sw = softwareList.find((s) => s.id === pkg.softwareId);
      setEditVersions(sw?.versions || []);
    } else {
      setEditVersions([]);
    }

    editForm.setFieldsValue({
      softwareId: pkg.softwareId,
      softwareVersionId: pkg.softwareVersionId,
      silentArgs: pkg.silentInstallArgs || '/qn /norestart',
    });
    setIsEditModalOpen(true);
  };

  const handleEditSubmit = async (values: any) => {
    if (!editingPackage) return;
    try {
      await api.put(`/Packages/${editingPackage.id}`, {
        softwareVersionId: values.softwareVersionId,
        silentArgs: values.silentArgs,
      });
      message.success('Paket bilgileri güncellendi.');
      setIsEditModalOpen(false);
      setEditingPackage(null);
      fetchPackages();
    } catch {
      message.error('Paket güncellenemedi.');
    }
  };

  const handleDeletePackage = async (id: string) => {
    try {
      await api.delete(`/Packages/${id}`);
      message.success('Paket bulut depodan silindi.');
      fetchPackages();
    } catch {
      message.error('Paket silinemedi.');
    }
  };

  const columns = [
    {
      title: 'Dosya Adı & Türü',
      dataIndex: 'fileName',
      key: 'fileName',
      render: (text: string, record: any) => (
        <Space direction="vertical" size={2}>
          <Space>
            <HddOutlined style={{ color: '#1677ff', fontSize: 16 }} />
            <Text strong>{text}</Text>
          </Space>
          <Space size={4}>
            <Tag color="blue">{record.installerType === 1 ? 'MSI' : 'EXE'}</Tag>
            <Tag color="geekblue">{record.architecture === 1 ? 'x64' : 'x86'}</Tag>
          </Space>
        </Space>
      ),
    },
    {
      title: 'İlişkili Yazılım',
      key: 'softwareInfo',
      render: (_: any, record: any) => (
        <Space direction="vertical" size={2}>
          <Space>
            <AppstoreOutlined style={{ color: '#52c41a' }} />
            <Text strong>{record.softwareName || 'Genel Paket'}</Text>
          </Space>
        </Space>
      ),
    },
    {
      title: 'Atanan Sürüm',
      dataIndex: 'version',
      key: 'version',
      render: (v: string) => <Tag color="green">v{v || '1.0'}</Tag>,
    },
    {
      title: 'Sessiz Kurulum Komutu',
      dataIndex: 'silentInstallArgs',
      key: 'silentInstallArgs',
      render: (args: string) => (
        <Tag icon={<CodeOutlined />} color="volcano" style={{ fontFamily: 'monospace' }}>
          {args || '/qn /norestart'}
        </Tag>
      ),
    },
    {
      title: 'Dosya Boyutu',
      dataIndex: 'fileSizeBytes',
      key: 'fileSizeBytes',
      render: (bytes: number) =>
        bytes ? (
          <Text type="secondary">{(bytes / (1024 * 1024)).toFixed(2)} MB</Text>
        ) : (
          '—'
        ),
    },
    {
      title: 'SHA-256 Hash',
      dataIndex: 'sha256Hash',
      key: 'sha256Hash',
      render: (hash: string) => (
        <Tooltip title={hash}>
          <Tag color="purple" icon={<FileProtectOutlined />}>
            {hash ? `${hash.substring(0, 10)}...` : 'N/A'}
          </Tag>
        </Tooltip>
      ),
    },
    {
      title: 'İşlemler',
      key: 'actions',
      render: (_: any, record: any) => (
        <Space>
          <Tooltip title="Düzenle / Sürüm Eşle">
            <Button
              size="small"
              icon={<EditOutlined />}
              onClick={() => openEditModal(record)}
            />
          </Tooltip>
          <Popconfirm
            title="Bu paketi silmek istediğinize emin misiniz?"
            onConfirm={() => handleDeletePackage(record.id)}
            okText="Evet, Sil"
            cancelText="İptal"
          >
            <Button danger size="small" icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Card
      title={
        <Space>
          <span>Merkezi Bulut Depo & Paket Yönetimi</span>
          <Badge count={packages.length} color="blue" />
        </Space>
      }
      extra={
        <Space>
          <Tooltip title="Yenile">
            <Button icon={<ReloadOutlined />} onClick={() => { fetchPackages(); fetchSoftware(); }} />
          </Tooltip>
          <Button
            type="primary"
            icon={<UploadOutlined />}
            onClick={() => setIsUploadModalOpen(true)}
          >
            Paket Yükle
          </Button>
        </Space>
      }
    >
      <Table
        dataSource={packages}
        columns={columns}
        rowKey="id"
        loading={loading}
        locale={{ emptyText: 'Henüz depoya paket yüklenmedi.' }}
      />

      {/* YÜKLEME MODALI */}
      <Modal
        title="Bulut Depoya Paket Yükle ve Sürümle Eşle"
        open={isUploadModalOpen}
        onCancel={() => {
          setIsUploadModalOpen(false);
          uploadForm.resetFields();
          setFileList([]);
          setUploadVersions([]);
        }}
        onOk={() => uploadForm.submit()}
        okText="Yükle"
        cancelText="İptal"
        width={560}
      >
        <Form form={uploadForm} layout="vertical" onFinish={handleUpload}>
          <Form.Item label="1. Paket Dosyası Seçin (.exe, .msi)">
            <Upload
              beforeUpload={(file) => {
                setFileList([file]);
                return false;
              }}
              fileList={fileList}
              maxCount={1}
            >
              <Button icon={<UploadOutlined />}>Dosya Seç (Gözat)</Button>
            </Upload>
          </Form.Item>

          <Form.Item label="2. Hedef Yazılımı Seçin">
            <Select
              placeholder="Yazılım Seçin (İsteğe Bağlı)"
              onChange={handleUploadSoftwareChange}
              allowClear
              showSearch
              optionFilterProp="children"
            >
              {softwareList.map((s) => (
                <Select.Option key={s.id} value={s.id}>
                  {s.name} {s.publisher ? `(${s.publisher})` : ''}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="softwareVersionId"
            label="3. Hedef Sürüm Seçin"
          >
            <Select
              placeholder={
                uploadVersions.length
                  ? 'Sürüm Seçin'
                  : 'Önce Yukarıdan Yazılım Seçin'
              }
              disabled={!uploadVersions.length}
              allowClear
            >
              {uploadVersions.map((v) => (
                <Select.Option key={v.id} value={v.id}>
                  v{v.version} {v.isCurrent ? '(Aktif Sürüm)' : ''}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="silentArgs"
            label="4. Sessiz Kurulum Parametresi (Silent Install)"
            initialValue="/qn /norestart"
          >
            <Input placeholder="Örn: /qn /norestart veya /S" />
          </Form.Item>
        </Form>
      </Modal>

      {/* DÜZENLEME MODALI */}
      <Modal
        title="Paket Bilgilerini ve Sürüm Atamasını Düzenle"
        open={isEditModalOpen}
        onCancel={() => {
          setIsEditModalOpen(false);
          setEditingPackage(null);
        }}
        onOk={() => editForm.submit()}
        okText="Kaydet"
        cancelText="İptal"
        width={560}
      >
        <Form form={editForm} layout="vertical" onFinish={handleEditSubmit}>
          <Form.Item label="Yazılım Değiştir / Eşle">
            <Select
              placeholder="Yazılım Seçin"
              onChange={handleEditSoftwareChange}
              allowClear
              showSearch
              optionFilterProp="children"
            >
              {softwareList.map((s) => (
                <Select.Option key={s.id} value={s.id}>
                  {s.name} {s.publisher ? `(${s.publisher})` : ''}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="softwareVersionId"
            label="Atanacak Sürüm"
          >
            <Select
              placeholder={
                editVersions.length ? 'Sürüm Seçin' : 'Önce Yazılım Seçin'
              }
              disabled={!editVersions.length}
              allowClear
            >
              {editVersions.map((v) => (
                <Select.Option key={v.id} value={v.id}>
                  v{v.version} {v.isCurrent ? '(Aktif)' : ''}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="silentArgs"
            label="Sessiz Kurulum Komutu (Silent Install Args)"
          >
            <Input placeholder="Örn: /qn /norestart veya /S" />
          </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
};
